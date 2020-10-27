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
using System.Runtime.InteropServices;
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
        public FastForestCandlestickModelParameters Parameters { get; set; }
        public CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters> Metadata { get; private set; }

        public bool IsTrained => Metadata.IsTrained;

        public MLStockFastForestCandlestickModel()
        {
            Metadata = new CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters>();
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new FastForestCandlestickModelParameters();

        }

        public Result Load(string pathToModel)
        {
            try
            {

                Metadata = JsonConvert.DeserializeObject<CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters>>(
                    File.ReadAllText($"{pathToModel}-Metadata.json"));

                DataViewSchema schema = null;
                _model = _mLContext.Model.Load($"{pathToModel}-Model.zip", out schema);

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
                new List<StockCandlestickDataFeature>()
                {
                    new StockCandlestickDataFeature(Array.ConvertAll(input.Data, y => (float)y), false, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 366.0f)
                },
                GetSchemaDefinition(input));

            var prediction = _model.Transform(inputDataView);

            var score = prediction.GetColumn<float>("Score").ToArray();
            //var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
            var probability = prediction.GetColumn<float>("Probability").ToArray();

            return new StockPrediction()
            {
                Score = score[0],
                PredictedLabel = score[0] > 0.0f,
                Probability = probability[0] * 100.0f
                //Probability = score[0]
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

                if (!Directory.Exists(pathToModel))
                {
                    Directory.CreateDirectory(pathToModel);
                }

                File.WriteAllText($"{pathToModel}-Metadata.json", JsonConvert.SerializeObject(Metadata, Formatting.Indented));
                _mLContext.Model.Save(_model, _dataSchema, $"{pathToModel}-Model.zip");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to save the model: {ex.Message}");
            }
        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction,
            ITrainingOutputMapper trainingOutputMapper)
        {
            if (!dataset.Any())
            {
                return Result.Failure<ModelMetrics>($"Training dataset is empty");
            }

            var firstFeature = dataset.FirstOrDefault();

            var rnd = new Random();
            int trainingCount = (int)((double)dataset.Count() * (1.0 - testFraction));
            var trainData = _mLContext.Data.LoadFromEnumerable(
                dataset.Take(trainingCount).Select(x =>
                {
                    var normVector = x.Input;
                    return new StockCandlestickDataFeature(
                    Array.ConvertAll(x.Input.Data, y => (float)y),
                    trainingOutputMapper.GetBinaryValue(x.Output),
                    trainingOutputMapper.GetOutputValue(x.Output),
                    x.Output.Symbol,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));
            var testData = _mLContext.Data.LoadFromEnumerable(
                dataset.Skip(trainingCount).Select(x =>
                {
                    var normVector = x.Input;
                    return new StockCandlestickDataFeature(
                    Array.ConvertAll(x.Input.Data, y => (float)y),
                    trainingOutputMapper.GetBinaryValue(x.Output),
                    trainingOutputMapper.GetOutputValue(x.Output),
                    x.Output.Symbol,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));

            _dataSchema = trainData.Schema;
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;

            var estimator = _mLContext.BinaryClassification.Trainers.FastForest(numberOfTrees: Parameters.NumOfTrees,
                numberOfLeaves: Parameters.NumOfLeaves, minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves)
                .Append(_mLContext.BinaryClassification.Calibrators.Platt());

            _model = estimator.Fit(trainData);

            Metadata.TrainingResults = new ModelMetrics();
            if (testData != null)
            {
                var testPredictions = _model.Transform(testData);
                var labels = testData.GetColumn<bool>("Label").ToArray();
                var values = testData.GetColumn<float>("Value").ToArray();
                var features = testData.GetColumn<float[]>("Features").ToArray();

                var predictionData = new List<(float Score, float Probability, bool PredictedLabel, bool ActualLabel)>();
                for(int i = 0; i < features.Length; ++i)
                {
                    var posS = Predict(new FeatureVector(Array.ConvertAll(features[i], y => (double)y), new DateTime(), firstFeature.Input.Encoding));
                    //var negS = Predict(new FeatureVector(Array.ConvertAll(features[i], y => (double)y), new DateTime(), firstFeature.Input.Encoding), false);

                    predictionData.Add(((float)posS.Score, values[i], posS.Score > 0.0, labels[i]));
                }

                predictionData = predictionData.OrderByDescending(x => x.Score).ToList();
                var runningAccuracy = new List<double>();
                double correct = 0.0;
                for(int i = 0; i < predictionData.Count; ++i)
                {
                    if(predictionData[i].PredictedLabel == predictionData[i].ActualLabel)
                    {
                        correct++;
                    }
                    runningAccuracy.Add(correct / (double)(i + 1));
                }


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
            var definedSchema = SchemaDefinition.Create(typeof(StockCandlestickDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            return definedSchema;
        }
    }
}
