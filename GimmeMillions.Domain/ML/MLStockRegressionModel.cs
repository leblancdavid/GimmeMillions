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

            var dataSplit = _mLContext.Data.TrainTestSplit(dataViewData, testFraction);
            var trainData = dataSplit.TrainSet;
            var testData = dataSplit.TestSet;


            //IEstimator<ITransformer> dataPrepEstimator = _mLContext.Transforms.Concatenate("Features", "Input");
            //ITransformer dataPrepTransformer = dataPrepEstimator.Fit(trainData);
           // IDataView transformedTrainingData = dataPrepTransformer.Transform(trainData);

            var sdcaEstimator = _mLContext.Regression.Trainers.Sdca();

            var trainedModel = sdcaEstimator.Fit(trainData);

            var testDataPredictions = trainedModel.Transform(testData);

            float[] scoreColumn = testDataPredictions.GetColumn<float>("Score").ToArray();
            float[] labelColumn = testDataPredictions.GetColumn<float>("Label").ToArray();

            // Extract model metrics and get RSquared
            var trainedModelMetrics = _mLContext.Regression.Evaluate(testDataPredictions);
            double rSquared = trainedModelMetrics.RSquared;

            return Result.Ok(new TrainingResult());

        }
    }
}
