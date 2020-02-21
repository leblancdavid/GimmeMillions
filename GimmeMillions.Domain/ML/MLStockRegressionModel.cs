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
            var definedSchema = SchemaDefinition.Create(typeof(FeatureVectorToStockDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Value.Select(x => 
                new FeatureVectorToStockDataFeature(x.Input.Data, (float)x.Output.PercentDayChange)), definedSchema);

            IDataView trainData = null, testData = null;
            if(testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(dataViewData, testFraction);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = dataViewData;
            }
            

            //var trainer = _mLContext.Regression.Trainers.LightGbm(new LightGbmRegressionTrainer.Options
            //{
            //    LabelColumnName = nameof(FeatureVectorToStockDataFeature.Label),
            //    FeatureColumnName = nameof(FeatureVectorToStockDataFeature.Features),
            //    // How many leaves a single tree should have.
            //    NumberOfLeaves = 4,
            //    // Each leaf contains at least this number of training data points.
            //    MinimumExampleCountPerLeaf = 1,
            //    // The step size per update. Using a large value might reduce the
            //    // training time but also increase the algorithm's numerical
            //    // stability.
            //    LearningRate = 0.001,
            //    Booster = new GossBooster.Options()
            //    {
            //        TopRate = 0.3,
            //        OtherRate = 0.2
            //    }
            //});
            //var trainedModel = trainer.Fit(trainData);

            var trainer = _mLContext.Regression.Trainers.FastTree(new FastTreeRegressionTrainer.Options
            {
                LabelColumnName = nameof(FeatureVectorToStockDataFeature.Label),
                FeatureColumnName = nameof(FeatureVectorToStockDataFeature.Features),
                NumberOfTrees = 100,
                NumberOfLeaves = 10,
                MinimumExampleCountPerLeaf = 1,
                LearningRate = 0.001,
                FeatureFirstUsePenalty = 0.1
            });
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
