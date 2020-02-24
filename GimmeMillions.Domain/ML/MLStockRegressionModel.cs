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
        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public MLStockRegressionModel(IFeatureDatasetService featureDatasetService, string symbol)
        {
            StockSymbol = symbol;
            _featureDatasetService = featureDatasetService;
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

            //Load the data into a view
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Value.Select(x => 
                new StockDailyValueDataFeature(x.Input.Data, (float)x.Output.PercentDayChange)), definedSchema);

            //Normalize it
            var normalizeTransform = _mLContext.Transforms.NormalizeMeanVariance("Features",
                useCdf: true).Fit(dataViewData);
            var transformedData = normalizeTransform.Transform(dataViewData);

            var pcaTransform = _mLContext.Transforms.ApproximatedKernelMap("Features",
                rank: 1000,
                generator: new GaussianKernel(gamma: 0.7f)).Fit(transformedData);
            var pcaTransformedData = pcaTransform.Transform(transformedData);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if(testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(pcaTransformedData, testFraction);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = pcaTransformedData;
            }

            //var trainer = _mLContext.Regression.Trainers.FastTree(new FastTreeRegressionTrainer.Options
            //{
            //    LabelColumnName = nameof(StockDailyValueDataFeature.Label),
            //    FeatureColumnName = nameof(StockDailyValueDataFeature.Features),
            //    NumberOfTrees = 100,
            //    NumberOfLeaves = 10,
            //    MinimumExampleCountPerLeaf = 1,
            //    LearningRate = 0.001,
            //    FeatureFirstUsePenalty = 0.1
            //});
            var trainer = _mLContext.Regression.Trainers.LightGbm(
                numberOfLeaves: 1000,
                minimumExampleCountPerLeaf: 0);

            //var trainer = _mLContext.Regression.Trainers.Sdca();
            var cvResults = _mLContext.Regression.CrossValidate(trainData, trainer, 5);
            var trainedModel = trainer.Fit(trainData);


            var trainingResult = new TrainingResult();
            var trainDataPredictions = trainedModel.Transform(trainData);
            float[] scoreColumn = trainDataPredictions.GetColumn<float>("Score").ToArray();
            float[] labelColumn = trainDataPredictions.GetColumn<float>("Label").ToArray();
            for(int i = 0; i < scoreColumn.Length; ++i)
            {
                if((scoreColumn[i] > 0 && labelColumn[i] > 0) || (scoreColumn[i] < 0 && labelColumn[i] < 0))
                {
                    trainingResult.TrainingAccuracy++;
                }
                trainingResult.TrainingError += Math.Abs(scoreColumn[i] - labelColumn[i]);
            }

            trainingResult.TrainingAccuracy /= (double)scoreColumn.Length;
            trainingResult.TrainingError /= (double)scoreColumn.Length;

            if (testData != null)
            {
                var testDataPredictions = trainedModel.Transform(testData);
                scoreColumn = testDataPredictions.GetColumn<float>("Score").ToArray();
                labelColumn = testDataPredictions.GetColumn<float>("Label").ToArray();
                for (int i = 0; i < scoreColumn.Length; ++i)
                {
                    if ((scoreColumn[i] > 0 && labelColumn[i] > 0) || (scoreColumn[i] < 0 && labelColumn[i] < 0))
                    {
                        trainingResult.ValidationAccuracy++;
                    }
                    trainingResult.ValidationError += Math.Abs(scoreColumn[i] - labelColumn[i]);
                }

                trainingResult.ValidationAccuracy /= (double)scoreColumn.Length;
                trainingResult.ValidationError /= (double)scoreColumn.Length;
            }
                
            return Result.Ok(trainingResult);

        }
    }
}
