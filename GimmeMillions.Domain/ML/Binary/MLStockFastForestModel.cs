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

namespace GimmeMillions.Domain.ML.Binary
{
    public class FastForestBinaryModelParameters
    {
        public int NumCrossValidations { get; set; }
        public int NumOfTrees { get; set; }
        public int NumOfLeaves { get; set; }
        public int MinNumOfLeaves { get; set; }
        public int FeatureSelectionRank { get; set; }
        public FastForestBinaryModelParameters()
        {
            NumCrossValidations = 10;
            NumOfTrees = 100;
            NumOfLeaves = 20;
            MinNumOfLeaves = 1;
            FeatureSelectionRank = 500;
        }

    }

    public class MLStockFastForestModel : IBinaryStockPredictionModel<FastForestBinaryModelParameters, FeatureVector>
    {
        private MLContext _mLContext;
        private int _seed;
        private FeatureFilterTransform _frequencyUsageTransform;
        private FeatureFilterTransform _maxDifferenceFilterTransform;
        private ITransformer _model;

        private DataViewSchema _dataSchema;
        private string _modelId = "FFModel-v1";
        public FastForestBinaryModelParameters Parameters { get; set; }
        public BinaryPredictionModelMetadata<FastForestBinaryModelParameters> Metadata { get; private set; }

        public string StockSymbol => Metadata.StockSymbol;

        public bool IsTrained => Metadata.IsTrained;

        public string Encoding => Metadata.FeatureEncoding;
        

        public MLStockFastForestModel()
        {
            Metadata = new BinaryPredictionModelMetadata<FastForestBinaryModelParameters>();
            Metadata.ModelId = "FFModel-v1";
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new FastForestBinaryModelParameters();

        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            try
            {
                string directory = $"{pathToModel}/{_modelId}/{encoding}";

                Metadata = JsonConvert.DeserializeObject<BinaryPredictionModelMetadata<FastForestBinaryModelParameters>>(
                    File.ReadAllText($"{ directory}/{symbol}-Metadata.json"));

                var maxDiffLoad = FeatureFilterTransform.LoadFromFile($"{ directory}/{Metadata.StockSymbol}-MaxDiffFilterTransform.json", _mLContext);
                if (maxDiffLoad.IsFailure)
                {
                    return Result.Failure(maxDiffLoad.Error);
                }
                _maxDifferenceFilterTransform = maxDiffLoad.Value;
                var usageLoad = FeatureFilterTransform.LoadFromFile($"{ directory}/{Metadata.StockSymbol}-FrequencyUsageTransform.json", _mLContext);
                if (usageLoad.IsFailure)
                {
                    return Result.Failure(usageLoad.Error);
                }
                _frequencyUsageTransform = usageLoad.Value;

                DataViewSchema schema = null;
                _model = _mLContext.Model.Load($"{directory}/{Metadata.StockSymbol}-Model.zip", out schema);

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
                    new StockRiseDataFeature(input.Data, false, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 366.0f)
                },
                GetSchemaDefinition(input));

            var prediction = _model.Transform(
                _maxDifferenceFilterTransform.Transform(
                    _frequencyUsageTransform.Transform(inputDataView)));

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

                File.WriteAllText($"{directory}/{Metadata.StockSymbol}-Metadata.json", JsonConvert.SerializeObject(Metadata, Formatting.Indented));
                _maxDifferenceFilterTransform.SaveToFile($"{ directory}/{ Metadata.StockSymbol}-MaxDiffFilterTransform.json");
                _frequencyUsageTransform.SaveToFile($"{ directory}/{ Metadata.StockSymbol}-FrequencyUsageTransform.json");
                _mLContext.Model.Save(_model, _dataSchema, $"{directory}/{Metadata.StockSymbol}-Model.zip");

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

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            var firstFeature = dataset.FirstOrDefault();

            //Load the data into a view
            var datasetView = _mLContext.Data.LoadFromEnumerable(
                dataset.Select(x =>
                {
                    var normVector = x.Input;
                    return new StockRiseDataFeature(
                    normVector.Data, x.Output.PercentDayChange >= 0,
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

            int filteredFeatureLength = (int)(firstFeature.Input.Length * 0.75);
            var frequencyUsageEstimator = new FeatureFrequencyUsageFilterEstimator(_mLContext, rank: filteredFeatureLength, skip: 0);
            _frequencyUsageTransform = frequencyUsageEstimator.Fit(trainData);
            var mostUsedData = _frequencyUsageTransform.Transform(trainData);

            var positiveResults = TryModel(mostUsedData, Parameters.FeatureSelectionRank, true);
            Console.WriteLine($"Positive Acc: {positiveResults.Accuracy}, AoC: {positiveResults.AreaUnderPrecisionRecallCurve}, PP: {positiveResults.PositivePrecision}");
            var negativeResults = TryModel(mostUsedData, Parameters.FeatureSelectionRank, false);
            Console.WriteLine($"Negative Acc: {negativeResults.Accuracy}, AoC: {negativeResults.AreaUnderPrecisionRecallCurve}, PP: {negativeResults.PositivePrecision}");
            bool usePositiveSort = true;
            Metadata.TrainingResults = positiveResults;
            if (positiveResults.AreaUnderPrecisionRecallCurve < negativeResults.AreaUnderPrecisionRecallCurve)
            {
                usePositiveSort = false;
                Metadata.TrainingResults = negativeResults;
            }

            var maxDifferenceFilterEstimator = new MaxDifferenceFeatureFilterEstimator(_mLContext,
                rank: Parameters.FeatureSelectionRank, positiveSort: usePositiveSort);
            _maxDifferenceFilterTransform = maxDifferenceFilterEstimator.Fit(mostUsedData);
            var maxDifferenceData = _maxDifferenceFilterTransform.Transform(mostUsedData);

            _dataSchema = trainData.Schema;
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;
            Metadata.StockSymbol = firstFeature.Output.Symbol;

            var ffEstimator = _mLContext.BinaryClassification.Trainers.FastForest(
                       numberOfLeaves: Parameters.NumOfLeaves,
                       numberOfTrees: Parameters.NumOfTrees,
                       minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves);

            _model = ffEstimator.Fit(maxDifferenceData);

            if (testData != null)
            {
                var testPredictions = _model.Transform(
                    _maxDifferenceFilterTransform.Transform(
                        _frequencyUsageTransform.Transform(testData)));

                var testResults = _mLContext.BinaryClassification.Evaluate(testPredictions);

                Metadata.TrainingResults = new ModelMetrics(testResults);
            }
            return Result.Ok<ModelMetrics>(Metadata.TrainingResults);
        }

        private ModelMetrics TryModel(IDataView data,
            int rank, 
            bool positiveSort)
        {
            var filteredData = (new MaxDifferenceFeatureFilterEstimator(_mLContext, rank: rank, positiveSort: positiveSort))
                .Fit(data).Transform(data);
            var pipeline = _mLContext.BinaryClassification.Trainers.FastTree();

            var cvResults = _mLContext.BinaryClassification.CrossValidateNonCalibrated(filteredData, pipeline, Parameters.NumCrossValidations);
            return CrossValidationResultsToMetrics(cvResults);
        }

        private ModelMetrics CrossValidationResultsToMetrics(IReadOnlyList<CrossValidationResult<BinaryClassificationMetrics>> crossValidationResults)
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
