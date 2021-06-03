using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GimmeMillions.Domain.ML
{
    public class StockRangePredictorModelV2Parameters
    {
        public StockRangePredictorModelV2Parameters()
        {
            
        }

    }
    public class MLStockRangePredictorModelV2 : IStockRangePredictor
    {
        private MLContext _mLContext; 
        private DataViewSchema _dataSchema;
        private int _seed;
        private ITransformer _lowRangeModel;
        private ITransformer _highRangeModel;
        private ITransformer _sentimentModel;

        public bool IsTrained => Metadata.IsTrained;
        public CandlestickPredictionModelMetadata<StockRangePredictorModelV2Parameters> Metadata { get; private set; }
        public StockRangePredictorModelV2Parameters Parameters { get; set; }
        public MLStockRangePredictorModelV2()
        {
            Metadata = new CandlestickPredictionModelMetadata<StockRangePredictorModelV2Parameters>();
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new StockRangePredictorModelV2Parameters();
        }

        public Result Load(string pathToModel)
        {
            try
            {
                Metadata = JsonConvert.DeserializeObject<CandlestickPredictionModelMetadata<StockRangePredictorModelV2Parameters>>(
                    File.ReadAllText($"{pathToModel}-Metadata.json"));

                DataViewSchema schema = null;
                _lowRangeModel = _mLContext.Model.Load($"{pathToModel}-LowModel.zip", out schema);
                _highRangeModel = _mLContext.Model.Load($"{pathToModel}-HighModel.zip", out schema);
                _sentimentModel = _mLContext.Model.Load($"{pathToModel}-SentimentModel.zip", out schema);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to load the model: {ex.Message}");
            }
        }

        public StockRangePrediction Predict(FeatureVector input)
        {
            //Load the data into a view
            var inputDataView = _mLContext.Data.LoadFromEnumerable(
                new List<StockCandlestickDataFeature>()
                {
                    new StockCandlestickDataFeature(Array.ConvertAll(input.Data, y => (float)y), false, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 366.0f)
                },
                GetSchemaDefinition(input));

            var lowP = _lowRangeModel.Transform(inputDataView);
            var lowScore = lowP.GetColumn<float>("Score").ToArray();

            var highP = _highRangeModel.Transform(inputDataView);
            var highScore = highP.GetColumn<float>("Score").ToArray();

            var sP = _sentimentModel.Transform(inputDataView);
            var sScore = sP.GetColumn<float>("Score").ToArray();
            //var sScore = sP.GetColumn<float>("Probability").ToArray();
            //var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
            //var probability = prediction.GetColumn<float>("Probability").ToArray();

            return new StockRangePrediction()
            {
                PredictedLow = lowScore[0],
                PredictedHigh = highScore[0],
                Sentiment = sScore[0] * 100.0
                //Sentiment = highScore[0] / (highScore[0] - lowScore[0]) * 100.0f
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
                _mLContext.Model.Save(_lowRangeModel, _dataSchema, $"{pathToModel}-LowModel.zip");
                _mLContext.Model.Save(_highRangeModel, _dataSchema, $"{pathToModel}-HighModel.zip");
                _mLContext.Model.Save(_sentimentModel, _dataSchema, $"{pathToModel}-SentimentModel.zip");

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to save the model: {ex.Message}");
            }
        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction, ITrainingOutputMapper trainingOutputMapper)
        {
            if (!dataset.Any())
            {
                return Result.Failure<ModelMetrics>($"Training dataset is empty");
            }

            dataset = dataset.OrderBy(x => x.Output.Date).ToList();

            var firstFeature = dataset.FirstOrDefault();
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;

            //TRAIN THE LOW RANGE PREDICTOR
            int trainingCount = (int)((double)dataset.Count() * (1.0 - testFraction));

            var rangeEstimator = _mLContext.Regression.Trainers.Gam(labelColumnName: "Value");
            var sentimateEstimator = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true)
                .Append(_mLContext.Regression.Trainers.Sdca(labelColumnName: "Value"));

            var trainLowData = _mLContext.Data.LoadFromEnumerable(
                dataset.Take(trainingCount).Select(x =>
                {
                    var normVector = x.Input;
                    return new StockCandlestickDataFeature(
                    Array.ConvertAll(x.Input.Data, y => (float)y),
                    trainingOutputMapper.GetBinaryValue(x.Output),
                   (float)x.Output.PercentChangeLowToPreviousClose,
                    x.Output.Symbol,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));
            var testLowData = _mLContext.Data.LoadFromEnumerable(
                dataset.Skip(trainingCount).Select(x =>
                {
                    var normVector = x.Input;
                    return new StockCandlestickDataFeature(
                    Array.ConvertAll(x.Input.Data, y => (float)y),
                    trainingOutputMapper.GetBinaryValue(x.Output),
                   (float)x.Output.PercentChangeLowToPreviousClose,
                    x.Output.Symbol,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));

            _dataSchema = trainLowData.Schema;
            _lowRangeModel = rangeEstimator.Fit(trainLowData);

            var trainHighData = _mLContext.Data.LoadFromEnumerable(
                dataset.Take(trainingCount).Select(x =>
                {
                    var normVector = x.Input;
                    return new StockCandlestickDataFeature(
                    Array.ConvertAll(x.Input.Data, y => (float)y),
                    trainingOutputMapper.GetBinaryValue(x.Output),
                    (float)x.Output.PercentChangeHighToPreviousClose,
                    x.Output.Symbol,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));
            var testHighData = _mLContext.Data.LoadFromEnumerable(
                dataset.Skip(trainingCount).Select(x =>
                {
                    var normVector = x.Input;
                    return new StockCandlestickDataFeature(
                    Array.ConvertAll(x.Input.Data, y => (float)y),
                    trainingOutputMapper.GetBinaryValue(x.Output),
                    (float)x.Output.PercentChangeHighToPreviousClose,
                    x.Output.Symbol,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));

            _highRangeModel = rangeEstimator.Fit(trainHighData);

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


            //var estimator = _mLContext.BinaryClassification.Trainers.LinearSvm(numberOfIterations: 500)
            //    .Append(_mLContext.BinaryClassification.Calibrators.Platt());
            //var estimator = _mLContext.BinaryClassification.Trainers.FastTree();

            _sentimentModel = sentimateEstimator.Fit(trainData);

            Metadata.TrainingResults = new ModelMetrics();
            if (testData != null)
            {
                var testPredictions = _sentimentModel.Transform(testData);
                var labels = testData.GetColumn<bool>("Label").ToArray();
                var values = testData.GetColumn<float>("Value").ToArray();
                var features = testData.GetColumn<float[]>("Features").ToArray();

                var predictionData = new List<(float Score, float Probability, bool PredictedLabel, bool ActualLabel)>();
                for (int i = 0; i < features.Length; ++i)
                {
                    var posS = Predict(new FeatureVector(Array.ConvertAll(features[i], y => (double)y), new DateTime(), firstFeature.Input.Encoding));
                    //var negS = Predict(new FeatureVector(Array.ConvertAll(features[i], y => (double)y), new DateTime(), firstFeature.Input.Encoding), false);

                    if (posS.Sentiment > 80.0f && (posS.PredictedHigh + posS.PredictedLow) > 0.0)
                        predictionData.Add(((float)posS.Sentiment, (float)values[i], true, values[i] > 0.5f));

                    if (posS.Sentiment < 20.0f && (posS.PredictedHigh + posS.PredictedLow) < 0.0)
                        predictionData.Add(((float)posS.Sentiment, (float)values[i], false, values[i] > 0.5f));

                    //if (Math.Abs(posS.PredictedHigh) > Math.Abs(posS.PredictedLow))
                    //    predictionData.Add(((float)posS.PredictedHigh, (float)values[i], true, values[i] > 0.5f));

                    //if (Math.Abs(posS.PredictedHigh) < Math.Abs(posS.PredictedLow))
                    //    predictionData.Add(((float)posS.PredictedHigh, (float)values[i], false, values[i] > 0.5f));
                }

                predictionData = predictionData.OrderByDescending(x => x.Score).ToList();
                var runningAccuracy = new List<double>();
                double correct = 0.0;
                for (int i = 0; i < predictionData.Count; ++i)
                {
                    if (predictionData[i].PredictedLabel == predictionData[i].ActualLabel)
                    {
                        correct++;
                    }
                    runningAccuracy.Add(correct / (double)(i + 1));
                }
           
            }
            return Result.Success<ModelMetrics>(Metadata.TrainingResults);
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
