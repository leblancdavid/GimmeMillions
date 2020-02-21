using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
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
        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public MLStockBinaryModel(IFeatureDatasetService featureDatasetService, string symbol)
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
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Value.Select(x =>
                new StockRiseDataFeature(x.Input.Data, x.Output.PercentDayChange >= 0, (float)x.Output.PercentDayChange)), definedSchema);

            IDataView trainData = null, testData = null;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(dataViewData, testFraction);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = dataViewData;
            }

            var trainer = _mLContext.BinaryClassification.Trainers.FastTree(
                numberOfLeaves: 20, 
                numberOfTrees: 100, 
                minimumExampleCountPerLeaf: 0);
            var trainedModel = trainer.Fit(trainData);

            var trainingResult = new TrainingResult();
            var trainDataPredictions = trainedModel.Transform(trainData);
            bool[] predictionColumn = trainDataPredictions.GetColumn<bool>("PredictedLabel").ToArray();
            bool[] labelColumn = trainDataPredictions.GetColumn<bool>("Label").ToArray();
            var probabilityColumn = trainDataPredictions.GetColumn<float>("Probability").ToArray();
            for (int i = 0; i < predictionColumn.Length; ++i)
            {
                if ((predictionColumn[i] && labelColumn[i]) || (!predictionColumn[i] && !labelColumn[i]))
                {
                    trainingResult.TrainingAccuracy++;
                }
            }

            trainingResult.TrainingAccuracy /= (double)predictionColumn.Length;

            if (testData != null)
            {
                var testDataPredictions = trainedModel.Transform(testData);
                predictionColumn = testDataPredictions.GetColumn<bool>("PredictedLabel").ToArray();
                labelColumn = testDataPredictions.GetColumn<bool>("Label").ToArray();
                probabilityColumn = testDataPredictions.GetColumn<float>("Probability").ToArray();
                var valueColumn = testDataPredictions.GetColumn<float>("Value").ToArray();
                for (int i = 0; i < predictionColumn.Length; ++i)
                {
                    if ((predictionColumn[i] && labelColumn[i]) || (!predictionColumn[i] && !labelColumn[i]))
                    {
                        trainingResult.ValidationAccuracy++;
                    }
                }

                trainingResult.ValidationAccuracy /= (double)predictionColumn.Length;
            }

            return Result.Ok(trainingResult);

        }
    }
}
