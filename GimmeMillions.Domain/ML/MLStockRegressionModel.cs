using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Trainers.LightGbm;
using Microsoft.ML.Transforms;

namespace GimmeMillions.Domain.ML
{
    public class MLStockRegressionModel : IStockPredictionModel
    {
        private IFeatureDatasetService _featureDatasetService;
        private MLContext _mLContext;
        private int _seed;
        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public MLStockRegressionModel(IFeatureDatasetService featureDatasetService, string symbol)
        {
            StockSymbol = symbol;
            _featureDatasetService = featureDatasetService;
            _seed = 27;
            _mLContext = new MLContext();

        }

        public Result Load(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<StockPrediction> Predict(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Result<StockPrediction> PredictLatest()
        {
            throw new NotImplementedException();
        }

        public Result Save(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<TrainingResult> Train(DateTime startDate, DateTime endDate, double testFraction)
        {
            var dataset = _featureDatasetService.GetTrainingData(StockSymbol, startDate, endDate);
            if(dataset.IsFailure)
            {
                return Result.Failure<TrainingResult>(dataset.Error);
            }

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            int featureDimension = dataset.Value.FirstOrDefault().Input.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockDailyValueDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            var averageNegativeValue = dataset.Value.Where(x => x.Output.PercentDayChange < 0)
                .Select(x => x.Output.PercentDayChange)
                .Average();
            //Load the data into a view
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Value.Select(x => 
                new StockDailyValueDataFeature(x.Input.Data, 
                x.Output.PercentDayChange > 0 ? (float)x.Output.PercentDayChange : (float)averageNegativeValue)), definedSchema);

            var normalizedData = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true)
                .Fit(dataViewData)
                .Transform(dataViewData);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if(testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(normalizedData, testFraction, seed: _seed);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = normalizedData;
            }

            int iterations = 10, crossValidations = 5;
            int numberOfTrees = dataset.Value.Count() / 30;
            int numberOfLeaves = numberOfTrees / 5;

            ApproximatedKernelTransformer bestPcaModel = null;
            double bestError = double.MaxValue;

            //var pca = _mLContext.Transforms.ProjectToPrincipalComponents(outputColumnName: "Features", rank: 20, overSampling: 20);
            var pca = _mLContext.Transforms.ApproximatedKernelMap(outputColumnName: "Features", rank: 20);

            for (int i = 0; i < iterations; ++i)
            {
                var pcaModel = pca.Fit(trainData);
                var pcaData = pcaModel.Transform(trainData);

                var forestTrainer = _mLContext.Regression.Trainers.FastForest(
                    numberOfLeaves: numberOfLeaves,
                    numberOfTrees: numberOfTrees,
                    minimumExampleCountPerLeaf: 1);
                var cvResults = _mLContext.Regression.CrossValidate(pcaData, forestTrainer, crossValidations);
                var averageError = cvResults.Select(x => x.Metrics.MeanAbsoluteError).Average();
                if(averageError < bestError)
                {
                    bestPcaModel = pcaModel;
                    bestError = averageError;
                }
            }

            var processedData = bestPcaModel.Transform(trainData);
            var model = _mLContext.Regression.Trainers.FastForest(
                    numberOfLeaves: numberOfLeaves,
                    numberOfTrees: numberOfTrees,
                    minimumExampleCountPerLeaf: 1).Fit(processedData);
            //Normalize it


            if (testData != null)
            {
                var testProcessedData = model.Transform(bestPcaModel.Transform(trainData));

                var labelsColumn = testProcessedData.GetColumn<float>("Label").ToArray();

                var results = _mLContext.Regression.Evaluate(testProcessedData);
                double accuracy = 0.0;
            }

            return Result.Ok(new TrainingResult());

        }
    }
}
