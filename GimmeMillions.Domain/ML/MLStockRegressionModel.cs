using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Binary;
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

            ApproximatedKernelTransformer bestPcaModel = null, worstPcaModel = null;

            List<ITransformer> bestModels = null, worstModels = null;
            double bestError = double.MaxValue, worstError = 0.0;

            //var pca = _mLContext.Transforms.ProjectToPrincipalComponents(outputColumnName: "Features", rank: 20, overSampling: 20);
            var pca = _mLContext.Transforms.ApproximatedKernelMap(outputColumnName: "Features", rank: 20);

            for (int i = 0; i < iterations; ++i)
            {
                var pcaModel = pca.Fit(trainData);
                var pcaData = pcaModel.Transform(trainData);

                //var trainer = _mLContext.Regression.Trainers.FastForest(
                //    numberOfLeaves: numberOfLeaves,
                //    numberOfTrees: numberOfTrees,
                //    minimumExampleCountPerLeaf: 1);
                var trainer = _mLContext.Regression.Trainers.Sdca();
                var cvResults = _mLContext.Regression.CrossValidate(pcaData, trainer, crossValidations);
                var averageError = cvResults.Select(x => x.Metrics.MeanAbsoluteError).Average();
                if(averageError < bestError)
                {
                    bestPcaModel = pcaModel;
                    bestModels = cvResults.Select(x => x.Model).ToList();
                    bestError = averageError;
                }

                if (averageError > worstError)
                {
                    worstPcaModel = pcaModel;
                    worstModels = cvResults.Select(x => x.Model).ToList();
                    worstError = averageError;
                }
            }
            //Normalize it


            if (testData != null)
            {
                double bestAvgError = 0.0;
                foreach(var m in bestModels)
                {
                    var betterResults = _mLContext.Regression.Evaluate(m.Transform(bestPcaModel.Transform(testData)));
                    bestAvgError += betterResults.MeanAbsoluteError;
                }
                bestAvgError /= bestModels.Count();

                double worstAvgError = 0.0;
                foreach (var m in worstModels)
                {
                    var worstResults = _mLContext.Regression.Evaluate(m.Transform(worstPcaModel.Transform(testData)));
                    worstAvgError += worstResults.MeanAbsoluteError;
                }
                worstAvgError /= worstModels.Count();

                //var betterResults = _mLContext.Regression.Evaluate(betterModel.Transform(bestPcaModel.Transform(testData)));
                //var worstResults = _mLContext.Regression.Evaluate(worstModel.Transform(worstModel.Transform(testData)));
            }

            return Result.Ok(new TrainingResult());

        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            throw new NotImplementedException();
        }

        public Result<StockPrediction> Predict(FeatureVector Input)
        {
            throw new NotImplementedException();
        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            throw new NotImplementedException();
        }

        StockPrediction IStockPredictionModel.Predict(FeatureVector Input)
        {
            throw new NotImplementedException();
        }
    }
}
