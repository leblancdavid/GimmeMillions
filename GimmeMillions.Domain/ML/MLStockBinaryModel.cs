using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class MLStockBinaryModel : IStockPredictionModel
    {
        private IFeatureDatasetService _featureDatasetService;
        private MLContext _mLContext;
        private int _seed;
        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public MLStockBinaryModel(IFeatureDatasetService featureDatasetService, string symbol)
        {
            StockSymbol = symbol;
            _featureDatasetService = featureDatasetService;
            _seed = 27;
            _mLContext = new MLContext(_seed);

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
            if (dataset.IsFailure)
            {
                return Result.Failure<TrainingResult>(dataset.Error);
            }

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            int featureDimension = dataset.Value.FirstOrDefault().Input.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            //Load the data into a view
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Value.Select(x =>
                new StockRiseDataFeature(x.Input.Data, x.Output.PercentDayChange >= 0, (float)x.Output.PercentDayChange)), definedSchema);

            var normalizedData = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true)
                .Fit(dataViewData)
                .Transform(dataViewData);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if (testFraction > 0.0)
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

            List<ITransformer> bestModels = null, worstModels = null;
            double bestPrecision = 0.0, worstPrecision = double.MaxValue;

            //var pca = _mLContext.Transforms.ProjectToPrincipalComponents(outputColumnName: "Features", rank: 20, overSampling: 20);
            for (int i = 0; i < iterations; ++i)
            {
                var trainer = _mLContext.Transforms.ApproximatedKernelMap(outputColumnName: "Features", rank: 20)
                    .Append( _mLContext.BinaryClassification.Trainers.FastTree(
                        numberOfLeaves: numberOfLeaves,
                        numberOfTrees: numberOfTrees,
                        minimumExampleCountPerLeaf: 1));
                //var trainer = _mLContext.Regression.Trainers.Sdca();
                var cvResults = _mLContext.BinaryClassification.CrossValidate(trainData, trainer, crossValidations);
                var averagePrecision = cvResults.Select(x => x.Metrics.PositivePrecision).Average();
                if (averagePrecision > bestPrecision)
                {
                    bestModels = cvResults.Select(x => x.Model).ToList();
                    bestPrecision = averagePrecision;
                }

                if (averagePrecision < worstPrecision)
                {
                    worstModels = cvResults.Select(x => x.Model).ToList();
                    worstPrecision = averagePrecision;
                }
            }

            if (testData != null)
            {
                double bestAvg = 0.0;
                foreach (var m in bestModels)
                {
                    var betterResults = _mLContext.BinaryClassification.Evaluate(m.Transform(testData));
                    bestAvg += betterResults.PositivePrecision;
                }
                bestAvg /= bestModels.Count();

                double worstAvg = 0.0;
                foreach (var m in worstModels)
                {
                    var worstResults = _mLContext.BinaryClassification.Evaluate(m.Transform(testData));
                    worstAvg += worstResults.PositivePrecision;
                }
                worstAvg /= worstModels.Count();

                //var betterResults = _mLContext.Regression.Evaluate(betterModel.Transform(bestPcaModel.Transform(testData)));
                //var worstResults = _mLContext.Regression.Evaluate(worstModel.Transform(worstModel.Transform(testData)));
            }

            return Result.Ok(new TrainingResult());

        }
    }
}
