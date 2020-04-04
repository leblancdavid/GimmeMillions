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
    public class KernelEstimationFastForestModelParameters
    {
        public int NumCrossValidations { get; set; }
        public int NumOfTrees { get; set; }
        public int NumOfLeaves { get; set; }
        public int MinNumOfLeaves { get; set; }
        public int FeatureSelectionRank { get; set; }
        public int KernelRank { get; set; }
        public int NumIterations { get; set; }
        public StockChangePointMethod ChangePoint { get; set; }
        public KernelEstimationFastForestModelParameters()
        {
            NumCrossValidations = 5;
            NumIterations = 10;
            NumOfTrees = 2000;
            NumOfLeaves = 20;
            MinNumOfLeaves = 1;
            FeatureSelectionRank = 500;
            KernelRank = 250;
            ChangePoint = StockChangePointMethod.PreviousCloseToClose;
        }

    }

    public class MLStockKernelEstimationFastForestModel 
        : IBinaryStockPredictionModel<KernelEstimationFastForestModelParameters, HistoricalFeatureVector>
    {
        private MLContext _mLContext;
        private int _seed;
        private FeatureFilterTransform _frequencyUsageTransform;
        private FeatureFilterTransform _maxDifferenceFilterTransform;
        private ITransformer _kernelTransform;
        private ITransformer _model;

        private DataViewSchema _dataSchema;
        private string _modelId = "KernelFFModel-v1";
        public KernelEstimationFastForestModelParameters Parameters { get; set; }
        public BinaryPredictionModelMetadata<KernelEstimationFastForestModelParameters> Metadata { get; private set; }

        public string StockSymbol => Metadata.StockSymbol;

        public bool IsTrained => Metadata.IsTrained;

        public string Encoding => Metadata.FeatureEncoding;
        

        public MLStockKernelEstimationFastForestModel()
        {
            Metadata = new BinaryPredictionModelMetadata<KernelEstimationFastForestModelParameters>();
            Metadata.ModelId = _modelId;
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new KernelEstimationFastForestModelParameters();

        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            try
            {
                string directory = $"{pathToModel}/{_modelId}/{encoding}";

                Metadata = JsonConvert.DeserializeObject<BinaryPredictionModelMetadata<KernelEstimationFastForestModelParameters>>(
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
                _kernelTransform = _mLContext.Model.Load($"{directory}/{Metadata.StockSymbol}-Kernel.zip", out schema);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to load the model: {ex.Message}");
            }
        }

        public StockPrediction Predict(HistoricalFeatureVector input)
        {
            //Load the data into a view
            var inputDataView = _mLContext.Data.LoadFromEnumerable(
                new List<StockRiseDataFeature>()
                {
                    new StockRiseDataFeature(input.Data, new float[0], false, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 366.0f)
                },
                GetSchemaDefinition(input));

            var prediction = _model.Transform(
                _kernelTransform.Transform(
                       _maxDifferenceFilterTransform.Transform(
                        _frequencyUsageTransform.Transform(inputDataView))));

            var score = prediction.GetColumn<float>("Score").ToArray();
            //var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
            var probability = prediction.GetColumn<float>("Probability").ToArray();

            return new StockPrediction()
            {
                Score = score[0],
                PredictedLabel = score[0] > 0.0f,
                Probability = probability[0]
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

                string directory = $"{pathToModel}/{_modelId}/{Metadata.FeatureEncoding}";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText($"{directory}/{Metadata.StockSymbol}-Metadata.json", JsonConvert.SerializeObject(Metadata, Formatting.Indented));
                _maxDifferenceFilterTransform.SaveToFile($"{ directory}/{ Metadata.StockSymbol}-MaxDiffFilterTransform.json");
                _frequencyUsageTransform.SaveToFile($"{ directory}/{ Metadata.StockSymbol}-FrequencyUsageTransform.json");
                _mLContext.Model.Save(_model, _dataSchema, $"{directory}/{Metadata.StockSymbol}-Model.zip");
                _mLContext.Model.Save(_kernelTransform, _dataSchema, $"{directory}/{Metadata.StockSymbol}-Kernel.zip");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to save the model: {ex.Message}");
            }
        }

        public Result<ModelMetrics> Train(IEnumerable<(HistoricalFeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            if (!dataset.Any())
            {
                return Result.Failure<ModelMetrics>($"Training dataset is empty");
            }

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            var firstFeature = dataset.FirstOrDefault();

            var stockTrainingData = dataset.Select(x =>
            {
                var normVector = x.Input;
                var value = GetLabelFromStockData(x.Output);
                return new StockRiseDataFeature(
                normVector.Data, new float[0], value > 0.0f,
                value,
                (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.DayOfYear / 366.0f);
            }).ToList();

            var meanValue = stockTrainingData.Average(x => x.Value);
            var stdDevValue = Math.Sqrt(stockTrainingData.Average(x => Math.Pow(x.Value - meanValue, 2)));

            var filteredStockTrainingData = stockTrainingData.Where(
                x => x.Value > meanValue - 0.5 * stdDevValue && x.Value < meanValue + 0.5 * stdDevValue).ToList();

            //Load the data into a view
            var datasetView = _mLContext.Data.LoadFromEnumerable(
                filteredStockTrainingData,
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

            int filteredFeatureLength = (int)(firstFeature.Input.Length * 0.8);
            var frequencyUsageEstimator = new FeatureFrequencyUsageFilterEstimator(_mLContext, rank: filteredFeatureLength, skip: 0);
            _frequencyUsageTransform = frequencyUsageEstimator.Fit(trainData);
            var mostUsedData = _frequencyUsageTransform.Transform(trainData);

            FeatureFilterTransform posFeatureTransform;
            ITransformer posKernelTransform;
            var positiveResults = TryModel(mostUsedData, Parameters.FeatureSelectionRank, true, out posFeatureTransform, out posKernelTransform);
            Console.WriteLine($"Positive Acc: {positiveResults.Accuracy}, AoC: {positiveResults.AreaUnderPrecisionRecallCurve}, PP: {positiveResults.PositivePrecision}");

            var negativeResults = TryModel(mostUsedData, Parameters.FeatureSelectionRank, false, out _maxDifferenceFilterTransform, out _kernelTransform);
            Console.WriteLine($"Negative Acc: {negativeResults.Accuracy}, AoC: {negativeResults.AreaUnderPrecisionRecallCurve}, PP: {negativeResults.PositivePrecision}");
            
            Metadata.TrainingResults = negativeResults;
            if (positiveResults.AreaUnderPrecisionRecallCurve > negativeResults.AreaUnderPrecisionRecallCurve)
            {
                Metadata.TrainingResults = positiveResults;
                _maxDifferenceFilterTransform = posFeatureTransform;
                _kernelTransform = posKernelTransform;
            }

            var maxDifferenceData = _maxDifferenceFilterTransform.Transform(mostUsedData);
            var kernelTransformedData = _kernelTransform.Transform(maxDifferenceData);

            _dataSchema = trainData.Schema;
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;
            Metadata.StockSymbol = firstFeature.Output.Symbol;
            var pipeline = _mLContext.BinaryClassification.Trainers.FastForest(
                       numberOfLeaves: Parameters.NumOfLeaves,
                       numberOfTrees: Parameters.NumOfTrees,
                       minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves)
                .Append(_mLContext.BinaryClassification.Calibrators.Platt());
            _model = pipeline.Fit(kernelTransformedData);

            if (testData != null)
            {
                var testPredictions = _model.Transform(
                    _kernelTransform.Transform(
                        _maxDifferenceFilterTransform.Transform(
                            _frequencyUsageTransform.Transform(testData))));

                var testResults = _mLContext.BinaryClassification.Evaluate(testPredictions);

                Metadata.TrainingResults = new ModelMetrics(testResults);
            }
            return Result.Ok<ModelMetrics>(Metadata.TrainingResults);
        }

        private ModelMetrics TryModel(IDataView data,
            int rank, 
            bool positiveSort, 
            out FeatureFilterTransform filterTransform,
            out ITransformer kernelTransform)
        {
            //filterTransform = null;
            filterTransform = new MaxDifferenceFeatureFilterEstimator(_mLContext, rank: rank, positiveSort: positiveSort).Fit(data);
            var filteredData =  filterTransform.Transform(data);

            double bestAccuracy = 0.0;
            ModelMetrics bestMetrics = null;
            kernelTransform = null;
            //var pcaEstimator = _mLContext.Transforms.ProjectToPrincipalComponents("Features", rank: Parameters.KernelRank, overSampling: Parameters.KernelRank);
            var pcaEstimator = _mLContext.Transforms.NormalizeMeanVariance("Features")
                .Append(_mLContext.Transforms.ApproximatedKernelMap("Features", rank: Parameters.KernelRank, useCosAndSinBases: false));
                //.Append(_mLContext.Transforms.NormalizeSupervisedBinning("Features", labelColumnName: "Value"));
            for (int i = 0; i < Parameters.NumIterations; ++i)
            {
                var kT = pcaEstimator.Fit(filteredData); 
                var kernelData = kT.Transform(filteredData);

                //var kdTest = kernelData.GetColumn<float[]>("Features").ToArray();

                var pipeline = _mLContext.BinaryClassification.Trainers.LinearSvm(numberOfIterations: 1);
                //var pipeline = _mLContext.BinaryClassification.Trainers.FastTree();
                var cvResults = CrossValidationResultsToMetrics(
                    _mLContext.BinaryClassification.CrossValidateNonCalibrated(kernelData, pipeline, Parameters.NumCrossValidations));

                //Console.WriteLine($"{positiveSort}({i}): {cvResults.Accuracy}");
                if(cvResults.Accuracy > bestAccuracy)
                {
                    kernelTransform = kT;
                    bestAccuracy = cvResults.Accuracy;
                    bestMetrics = cvResults;
                }
            }

            return bestMetrics;
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

        private SchemaDefinition GetSchemaDefinition(HistoricalFeatureVector vector)
        {
            int featureDimension = vector.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            return definedSchema;
        }

        private float GetLabelFromStockData(StockData stockData)
        {
            switch (Parameters.ChangePoint)
            {
                case StockChangePointMethod.PreviousCloseToHigh:
                    return (float)stockData.PercentChangeHighToPreviousClose;
                case StockChangePointMethod.PreviousCloseToLow:
                    return (float)stockData.PercentChangeLowToPreviousClose;
                case StockChangePointMethod.PreviousCloseToOpen:
                    return (float)stockData.PercentChangeOpenToPreviousClose;
                case StockChangePointMethod.PreviousCloseToClose:
                default:
                    return (float)stockData.PercentChangeFromPreviousClose;

            }
        }

    }
}
