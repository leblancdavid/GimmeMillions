using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Transforms;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Microsoft.ML.TrainCatalogBase;

namespace GimmeMillions.Domain.ML.Candlestick
{
    public class FastForestCandlestickModelParameters
    {
        public int NumCrossValidations { get; set; }
        public int NumOfTrees { get; set; }
        public int NumOfLeaves { get; set; }
        public int MinNumOfLeaves { get; set; }
        public FastForestCandlestickModelParameters()
        {
            NumCrossValidations = 10;
            NumOfTrees = 100;
            NumOfLeaves = 20;
            MinNumOfLeaves = 1;
        }

    }

    public class MLStockFastForestCandlestickModel : 
        ICandlestickStockPredictionModel<FastForestCandlestickModelParameters, FeatureVector>
    {
        private MLContext _mLContext;
        private int _seed;
        private ITransformer _model;

        private DataViewSchema _dataSchema;
        private string _modelId = "FFCandlestickModel-v1";
        public FastForestCandlestickModelParameters Parameters { get; set; }
        public CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters> Metadata { get; private set; }

        public string StockSymbol { get; set; }

        public bool IsTrained => Metadata.IsTrained;

        public string Encoding => Metadata.FeatureEncoding;
        

        public MLStockFastForestCandlestickModel()
        {
            Metadata = new CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters>();
            Metadata.ModelId = _modelId;
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new FastForestCandlestickModelParameters();

        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            try
            {
                string directory = $"{pathToModel}/{_modelId}/{encoding}";

                Metadata = JsonConvert.DeserializeObject<CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters>>(
                    File.ReadAllText($"{ directory}/Metadata.json"));

                DataViewSchema schema = null;
                _model = _mLContext.Model.Load($"{directory}/Model.zip", out schema);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to load the model: {ex.Message}");
            }
        }

        public StockPrediction Predict(FeatureVector input)
        {
            //Load the data into a view
            var inputDataView = _mLContext.Data.LoadFromEnumerable(
                new List<StockRiseDataFeature>()
                {
                    new StockRiseDataFeature(input.Data, new double[0], false, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 366.0f)
                },
                GetSchemaDefinition(input));

            var prediction = _model.Transform(inputDataView);

            var score = prediction.GetColumn<float>("Score").ToArray();
            //var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
           // var probability = prediction.GetColumn<float>("Probability").ToArray();

            return new StockPrediction()
            {
                Score = score[0],
                PredictedLabel = score[0] > 0.0f,
                //Probability = probability[0]
                Probability = score[0]
            };
        }

        public Result Save(string pathToModel)
        {
            try
            {
                if (!Metadata.IsTrained)
                {
                    return Result.Failure("Model has not been trained or loaded");
                }

                string directory = $"{pathToModel}/{_modelId}/{Metadata.FeatureEncoding}";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText($"{directory}/Metadata.json", JsonConvert.SerializeObject(Metadata, Formatting.Indented));
                _mLContext.Model.Save(_model, _dataSchema, $"{directory}/Model.zip");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to save the model: {ex.Message}");
            }
        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            if (!dataset.Any())
            {
                return Result.Failure<ModelMetrics>($"Training dataset is empty");
            }

            var firstFeature = dataset.FirstOrDefault();

            //Load the data into a view
            var datasetView = _mLContext.Data.LoadFromEnumerable(
                dataset.Select(x =>
                {
                    var normVector = x.Input;
                    return new StockRiseDataFeature(
                    normVector.Data,
                    new double[0],
                    x.Output.PercentDayChange >= 0,
                    (float)x.Output.PercentDayChange,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));

            IDataView trainData = null; //= dataSplit.TrainSet;
            IDataView testData = null; // dataSplit.TestSet;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(datasetView, testFraction: testFraction);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = datasetView;
            }

            
            _dataSchema = trainData.Schema;
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;

            var ffEstimator = _mLContext.BinaryClassification.Trainers.FastTree(
                       numberOfLeaves: Parameters.NumOfLeaves,
                       numberOfTrees: Parameters.NumOfTrees,
                       minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves);

            Metadata.TrainingResults = CrossValidationResultsToMetrics(
                _mLContext.BinaryClassification.CrossValidate(
                    trainData, ffEstimator, numberOfFolds: Parameters.NumCrossValidations));

            _model = ffEstimator.Fit(trainData);

            if (testData != null)
            {
                var testPredictions = _model.Transform(testData);

                var testResults = _mLContext.BinaryClassification.Evaluate(testPredictions);

                Metadata.TrainingResults = new ModelMetrics(testResults);
            }
            return Result.Ok<ModelMetrics>(Metadata.TrainingResults);
        }

        private ModelMetrics CrossValidationResultsToMetrics<T>(IReadOnlyList<CrossValidationResult<T>> crossValidationResults)
            where T : BinaryClassificationMetrics
        {
            var metrics = new ModelMetrics();
            foreach (var fold in crossValidationResults)
            {
                metrics.Accuracy += fold.Metrics.Accuracy;
                metrics.AreaUnderPrecisionRecallCurve += fold.Metrics.AreaUnderPrecisionRecallCurve;
                metrics.AreaUnderRocCurve += fold.Metrics.AreaUnderRocCurve;
                metrics.F1Score += fold.Metrics.F1Score;
                metrics.NegativePrecision += fold.Metrics.NegativePrecision;
                metrics.NegativeRecall += fold.Metrics.NegativeRecall;
                metrics.PositivePrecision += fold.Metrics.PositivePrecision;
                metrics.PositiveRecall += fold.Metrics.PositiveRecall;
            }

            metrics.Accuracy /= crossValidationResults.Count();
            metrics.AreaUnderPrecisionRecallCurve /= crossValidationResults.Count();
            metrics.AreaUnderRocCurve /= crossValidationResults.Count();
            metrics.F1Score /= crossValidationResults.Count();
            metrics.NegativePrecision /= crossValidationResults.Count();
            metrics.NegativeRecall /= crossValidationResults.Count();
            metrics.PositivePrecision /= crossValidationResults.Count();
            metrics.PositiveRecall /= crossValidationResults.Count();

            return metrics;
        }

        private SchemaDefinition GetSchemaDefinition(FeatureVector vector)
        {
            int featureDimension = vector.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            return definedSchema;
        }

    }
}
