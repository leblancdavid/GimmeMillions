﻿using CSharpFunctionalExtensions;
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
                new List<StockCandlestickDataFeature>()
                {
                    new StockCandlestickDataFeature(Array.ConvertAll(input.Data, y => (float)y), false, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 366.0f)
                },
                GetSchemaDefinition(input));

            var prediction = _model.Transform(inputDataView);

            var score = prediction.GetColumn<float>("Score").ToArray();
            //var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
            //var probability = prediction.GetColumn<float>("Probability").ToArray();

            return new StockPrediction()
            {
                Score = score[0],
                PredictedLabel = score[0] > 0.0f,
                //Probability = probability[0]
                Probability = score[0]
            };
        }

        private StockPrediction Predict(FeatureVector input, bool group)
        {
            //Load the data into a view
            var inputDataView = _mLContext.Data.LoadFromEnumerable(
                new List<StockCandlestickDataFeature>()
                {
                    new StockCandlestickDataFeature(Array.ConvertAll(input.Data, y => (float)y), group, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 366.0f)
                },
                GetSchemaDefinition(input));

            var prediction = _model.Transform(inputDataView);

            var score = prediction.GetColumn<float>("Score").ToArray();
            var groupId = prediction.GetColumn<uint>("GroupId").ToArray();
            //var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
            //var probability = prediction.GetColumn<float>("Probability").ToArray();

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

            var maxValue = dataset.Max(x => x.Output.PercentDayChange);
            var minValue = dataset.Min(x => x.Output.PercentDayChange);
            //var filteredDatasetPositives = dataset.Where(x => Math.Abs(x.Output.PercentDayChange) < 1.0m);
            //var medianPercent = GetMedian(filteredDatasetPositives.Select(x => x.Output.PercentDayChange).ToArray());
            //Load the data into a view
            var datasetViewPos = _mLContext.Data.LoadFromEnumerable(
                dataset.Select(x =>
                {
                    var normVector = x.Input;
                    //var v = ((x.Output.PercentDayChange - minValue) / (maxValue - minValue));
                    var v = GetRankValue(x.Output.PercentDayChange, -5.0m, 5.0m);
                    int rank = GetRank(x.Output.PercentDayChange, -10.0m, 10.0m);
                    return new StockCandlestickDataFeature(
                    Array.ConvertAll(x.Input.Data, y => (float)y),
                    x.Output.PercentDayChange > 0.0m,
                    (float)x.Output.PercentDayChange,
                    x.Output.PercentDayChange > 0.0m ? 1 : 0,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
                }),
                GetSchemaDefinition(firstFeature.Input));

            IDataView trainData = null; //= dataSplit.TrainSet;
            IDataView testData = null; // dataSplit.TestSet;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(datasetViewPos, testFraction: testFraction);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = datasetViewPos;
            }

            
            _dataSchema = trainData.Schema;
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;

            //var estimator = _mLContext.BinaryClassification.Trainers.FastForest(
            //           numberOfLeaves: Parameters.NumOfLeaves,
            //           numberOfTrees: Parameters.NumOfTrees,
            //           minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves);

            var estimator = _mLContext.Regression.Trainers.FastTree(
                       "Value", "Features");
                //.Append(_mLContext.BinaryClassification.Calibrators.Platt());

            //var estimator = _mLContext.BinaryClassification.Trainers.FastForest();

            //Metadata.TrainingResults = CrossValidationResultsToMetrics(
            //    _mLContext.Ranking.Cross(
            //        trainData, estimator, numberOfFolds: Parameters.NumCrossValidations));

            _model = estimator.Fit(trainData);

            if (testData != null)
            {
                //var testPredictions = _model.Transform(testData);
                //var testResults = _mLContext.Ranking.Evaluate(testPredictions);
                //Metadata.TrainingResults = new ModelMetrics(testResults);

               // var scores = testPredictions.GetColumn<float>("Score").ToArray();
                //var probabilities = testPredictions.GetColumn<float>("Probability").ToArray();
                //var predictedLabels = testPredictions.GetColumn<bool>("PredictedLabel").ToArray();
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

        public decimal GetMedian(decimal[] sourceNumbers)
        {
            //Framework 2.0 version of this method. there is an easier way in F4        
            if (sourceNumbers == null || sourceNumbers.Length == 0)
                throw new System.Exception("Median of empty array not defined.");

            //make sure the list is sorted, but use a new array
            decimal[] sortedPNumbers = (decimal[])sourceNumbers.Clone();
            Array.Sort(sortedPNumbers);

            //get the median
            int size = sortedPNumbers.Length;
            int mid = size / 2;
            decimal median = (size % 2 != 0) ? (decimal)sortedPNumbers[mid] : ((decimal)sortedPNumbers[mid] + (decimal)sortedPNumbers[mid - 1]) / 2;
            return median;
        }

        public int GetRank(decimal number, decimal min, decimal max)
        {
            var n = Math.Abs(number);
            if(n < min)
            {
                n = min;
            }
            if(n > max)
            {
                n = max;
            }

            return (int)Math.Ceiling((n - min) - 0.5m);
        }

        public float GetRankValue(decimal number, decimal min, decimal max)
        {
            var n = number;
            if (n < min)
            {
                n = min;
            }
            if (n > max)
            {
                n = max;
            }

            return (float)((n - min)/(max - min));
        }
    }
}
