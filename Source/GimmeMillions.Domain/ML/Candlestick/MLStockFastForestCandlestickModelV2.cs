﻿using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
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
    public class MLStockFastForestCandlestickModelV2 : 
        ICandlestickStockPredictionModel<FastForestCandlestickModelParameters, FeatureVector>
    {
        private MLContext _mLContext;
        private int _seed;
        private ITransformer _model;

        private DataViewSchema _dataSchema;
        private string _modelId = "FFCandlestickModel-v2";
        //private string _modelId = "SVMCandlestickModel-v1";
        public FastForestCandlestickModelParameters Parameters { get; set; }
        public CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters> Metadata { get; private set; }

        public bool IsTrained => Metadata.IsTrained;
        

        public MLStockFastForestCandlestickModelV2()
        {
            Metadata = new CandlestickPredictionModelMetadata<FastForestCandlestickModelParameters>();
            Metadata.ModelId = _modelId;
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

                return Result.Success();
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

           
                if (!Directory.Exists(pathToModel))
                {
                    Directory.CreateDirectory(pathToModel);
                }

                File.WriteAllText($"{pathToModel}-Metadata.json", JsonConvert.SerializeObject(Metadata, Formatting.Indented));
                _mLContext.Model.Save(_model, _dataSchema, $"{pathToModel}-Model.zip");

                return Result.Success();
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

            //var estimator = _mLContext.Regression.Trainers.FastForest(labelColumnName: "Value", numberOfTrees: Parameters.NumOfTrees,
            //      numberOfLeaves: Parameters.NumOfLeaves, minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves);
            //var estimator = _mLContext.Regression.Trainers.Sdca(labelColumnName: "Value");
            //var estimator = _mLContext.Transforms.NormalizeGlobalContrast("Features")
            //.Append(_mLContext.Transforms.ProjectToPrincipalComponents("Features", rank: 20, overSampling: 20))
            //    .Append(_mLContext.Regression.Trainers.Gam(labelColumnName: "Value"));
            //var estimator = _mLContext.Transforms.ProjectToPrincipalComponents("Features", rank: 20, overSampling: 20)
            //    .Append(_mLContext.Regression.Trainers.Sdca(labelColumnName: "Value"));
            //var estimator = _mLContext.Regression.Trainers.Gam(labelColumnName: "Value");

            //var estimator = _mLContext.BinaryClassification.Trainers.FastForest(numberOfTrees: Parameters.NumOfTrees,
            //      numberOfLeaves: Parameters.NumOfLeaves, minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves);
            //var estimator = _mLContext.BinaryClassification.Trainers.LinearSvm(numberOfIterations:10)
            //   .Append(_mLContext.BinaryClassification.Calibrators.Platt());
            //var estimator = _mLContext.Regression.Trainers.Gam(labelColumnName: "Value");
            var estimator = _mLContext.Regression.Trainers.FastForest(labelColumnName: "Value", numberOfTrees: Parameters.NumOfTrees,
                numberOfLeaves: Parameters.NumOfLeaves, minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves);
            _model = estimator.Fit(trainData);

            Metadata.TrainingResults = new ModelMetrics();
            if (testData != null)
            {
                var testPredictions = _model.Transform(testData);
                var labels = testData.GetColumn<bool>("Label").ToArray();
                var values = testData.GetColumn<float>("Value").ToArray();
                var features = testData.GetColumn<float[]>("Features").ToArray();

                var predictionData = new List<(float Score, float ActualScore, bool PredictedLabel, bool ActualLabel)>();
                for(int i = 0; i < features.Length; ++i)
                {
                    var posS = Predict(new FeatureVector(Array.ConvertAll(features[i], y => (double)y), new DateTime(), firstFeature.Input.Encoding));
                    //var negS = Predict(new FeatureVector(Array.ConvertAll(features[i], y => (double)y), new DateTime(), firstFeature.Input.Encoding), false);
                    //if(Math.Abs(posS.Score) > 0.75)
                        predictionData.Add(((float)posS.Score, values[i], posS.Score > 0.0, labels[i]));
                }

                predictionData = predictionData.OrderByDescending(x => x.Score).ToList();
                var runningAccuracy = new List<double>();
                var runningError = new List<double>();
                var totalError = 0.0;
                double correct = 0.0;
                for(int i = 0; i < predictionData.Count; ++i)
                {
                    if(predictionData[i].PredictedLabel == predictionData[i].ActualLabel)
                    {
                        correct++;
                    }
                    totalError += Math.Abs(predictionData[i].Score - predictionData[i].ActualScore);
                    runningAccuracy.Add(correct / (double)(i + 1));
                    runningError.Add(totalError / (double)(i + 1));
                }


            }
            return Result.Success<ModelMetrics>(Metadata.TrainingResults);
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

        public uint GetRank(decimal number, decimal min, decimal max)
        {
            if(number < min)
                return 1;
            if(number > max)
                return 4;
            if (number > 0.0m)
                return 3;
            else
                return 2;
        }

        public float GetValue(decimal number, decimal min, decimal max)
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
