using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Binary;
using GimmeMillions.Domain.ML.Transforms;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.TrainCatalogBase;

namespace GimmeMillions.Domain.ML.Regression
{
    public class RegressionKernelEstimationLinearModelParameters
    {
        public enum StockRegressionPointMethod
        {
            PreviousCloseToClose,
            PreviousCloseToHigh,
            PreviousCloseToLow
        }

        public StockRegressionPointMethod RegressionPoint { get; set; }
        public int NumCrossValidations { get; set; }
        public int FeatureSelectionRank { get; set; }
        public int KernelRank { get; set; }
        public int NumIterations { get; set; }
        public RegressionKernelEstimationLinearModelParameters()
        {
            NumCrossValidations = 5;
            NumIterations = 10;
            FeatureSelectionRank = 500;
            KernelRank = 250;
            RegressionPoint = StockRegressionPointMethod.PreviousCloseToClose;
        }

    }

    public class MLRegressionStockKernelEstimationLinearModel : IRegressionStockPredictionModel
    {
        private MLContext _mLContext;
        private int _seed;
        private FeatureFilterTransform _frequencyUsageTransform;
        private FeatureFilterTransform _maxDifferenceFilterTransform;
        private ITransformer _kernelTransform;
        private ITransformer _model;

        private DataViewSchema _dataSchema;
        private string _modelId = "KernelSvmRegressionModel-v1";
        public RegressionKernelEstimationLinearModelParameters Parameters { get; set; }

        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public string Encoding { get; private set; }


        public MLRegressionStockKernelEstimationLinearModel()
        {
            //Metadata = new BinaryPredictionModelMetadata<KernelEstimationSvmModelParameters>();
            //Metadata.ModelId = _modelId;
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new RegressionKernelEstimationLinearModelParameters();

        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            try
            {
                string directory = $"{pathToModel}/{_modelId}/{encoding}";

                //Metadata = JsonConvert.DeserializeObject<BinaryPredictionModelMetadata<KernelEstimationSvmModelParameters>>(
                //    File.ReadAllText($"{ directory}/{symbol}-Metadata.json"));

                var maxDiffLoad = FeatureFilterTransform.LoadFromFile($"{ directory}/{StockSymbol}-MaxDiffFilterTransform.json", _mLContext);
                if (maxDiffLoad.IsFailure)
                {
                    return Result.Failure(maxDiffLoad.Error);
                }
                _maxDifferenceFilterTransform = maxDiffLoad.Value;
                var usageLoad = FeatureFilterTransform.LoadFromFile($"{ directory}/{StockSymbol}-FrequencyUsageTransform.json", _mLContext);
                if (usageLoad.IsFailure)
                {
                    return Result.Failure(usageLoad.Error);
                }
                _frequencyUsageTransform = usageLoad.Value;

                DataViewSchema schema = null;
                _model = _mLContext.Model.Load($"{directory}/{StockSymbol}-Model.zip", out schema);
                _kernelTransform = _mLContext.Model.Load($"{directory}/{StockSymbol}-Kernel.zip", out schema);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to load the model: {ex.Message}");
            }
        }

        public StockRegressionPrediction Predict(FeatureVector input)
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
                _kernelTransform.Transform(
                    _maxDifferenceFilterTransform.Transform(
                        _frequencyUsageTransform.Transform(inputDataView))));

            var score = prediction.GetColumn<float>("Score").ToArray();
            //var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
            // var probability = prediction.GetColumn<float>("Probability").ToArray();

            return new StockRegressionPrediction()
            {
                //Score = score[0],
                //PredictedLabel = score[0] > 0.0f,
                ////Probability = probability[0]
                //Probability = score[0]
            };
        }

        public Result Save(string pathToModel)
        {
            try
            {
                if (!IsTrained)
                {
                    return Result.Failure("Model has not been trained or loaded");
                }

                string directory = $"{pathToModel}/{_modelId}/{Encoding}";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //File.WriteAllText($"{directory}/{Metadata.StockSymbol}-Metadata.json", JsonConvert.SerializeObject(Metadata, Formatting.Indented));
                _maxDifferenceFilterTransform.SaveToFile($"{ directory}/{StockSymbol}-MaxDiffFilterTransform.json");
                _frequencyUsageTransform.SaveToFile($"{ directory}/{StockSymbol}-FrequencyUsageTransform.json");
                _mLContext.Model.Save(_model, _dataSchema, $"{directory}/{StockSymbol}-Model.zip");
                _mLContext.Model.Save(_kernelTransform, _dataSchema, $"{directory}/{StockSymbol}-Kernel.zip");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to save the model: {ex.Message}");
            }
        }

        public Result<MLRegressionMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            if (!dataset.Any())
            {
                return Result.Failure<MLRegressionMetrics>($"Training dataset is empty");
            }

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            var firstFeature = dataset.FirstOrDefault();

            //Load the data into a view
            var datasetView = _mLContext.Data.LoadFromEnumerable(
                dataset.Select(x =>
                {
                    var normVector = x.Input;
                    return new StockRegressionDataFeature(
                    normVector.Data,
                    GetLabelFromStockData(x.Output),
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

            FeatureFilterTransform posFeatureTransform;
            ITransformer posKernelTransform;
            var positiveResults = TryModel(mostUsedData, Parameters.FeatureSelectionRank, true, out posFeatureTransform, out posKernelTransform);
            Console.WriteLine($"Positive MAE: {positiveResults.MeanAbsoluteError}, R2: {positiveResults.RSquared}");

            var negativeResults = TryModel(mostUsedData, Parameters.FeatureSelectionRank, false, out _maxDifferenceFilterTransform, out _kernelTransform);
            Console.WriteLine($"Negative Acc: {negativeResults.MeanAbsoluteError}, AoC: {negativeResults.RSquared}");

            //Metadata.TrainingResults = negativeResults;
            if (positiveResults.MeanAbsoluteError < negativeResults.MeanAbsoluteError)
            {
                //Metadata.TrainingResults = positiveResults;
                _maxDifferenceFilterTransform = posFeatureTransform;
                _kernelTransform = posKernelTransform;
            }

            var maxDifferenceData = _maxDifferenceFilterTransform.Transform(mostUsedData);
            var kernelTransformedData = _kernelTransform.Transform(maxDifferenceData);

            _dataSchema = trainData.Schema;
            Encoding = firstFeature.Input.Encoding;
            StockSymbol = firstFeature.Output.Symbol;

            var pipeline = _mLContext.Regression.Trainers.LbfgsPoissonRegression();
            _model = pipeline.Fit(kernelTransformedData);

            if (testData != null)
            {
                var testPredictions = _model.Transform(
                    _kernelTransform.Transform(
                        _maxDifferenceFilterTransform.Transform(
                            _frequencyUsageTransform.Transform(testData))));

                var testResults = _mLContext.Regression.Evaluate(testPredictions);

                //Metadata.TrainingResults = new ModelMetrics(testResults);
            }
            return Result.Ok<MLRegressionMetrics>(new MLRegressionMetrics());
        }

        private MLRegressionMetrics TryModel(IDataView data,
            int rank,
            bool positiveSort,
            out FeatureFilterTransform filterTransform,
            out ITransformer kernelTransform)
        {
            filterTransform = new MaxDifferenceFeatureFilterRegressionEstimator(_mLContext, rank: rank, positiveSort: positiveSort).Fit(data);
            var filteredData = filterTransform.Transform(data);

            double bestError = double.MaxValue;
            MLRegressionMetrics bestMetrics = null;
            kernelTransform = null;
            //var pcaEstimator = _mLContext.Transforms.ProjectToPrincipalComponents("Features", rank: Parameters.KernelRank, overSampling: Parameters.KernelRank);
            var pcaEstimator = _mLContext.Transforms.NormalizeMeanVariance("Features")
                .Append(_mLContext.Transforms.ApproximatedKernelMap("Features", rank: Parameters.KernelRank, useCosAndSinBases: true));
            for (int i = 0; i < Parameters.NumIterations; ++i)
            {
                var kT = pcaEstimator.Fit(filteredData);
                var kernelData = kT.Transform(filteredData);

                //var kdTest = kernelData.GetColumn<float[]>("Features").ToArray();

                var pipeline = _mLContext.Regression.Trainers.LbfgsPoissonRegression();
                //var pipeline = _mLContext.BinaryClassification.Trainers.FastTree();
                var cvResults = CrossValidationResultsToMetrics(
                    _mLContext.Regression.CrossValidate(kernelData, pipeline, Parameters.NumCrossValidations));

                Console.WriteLine($"{positiveSort}({i}): {cvResults.MeanAbsoluteError}");
                if (cvResults.MeanAbsoluteError < bestError)
                {
                    kernelTransform = kT;
                    bestError = cvResults.MeanAbsoluteError;
                    bestMetrics = cvResults;
                }
            }

            return bestMetrics;
        }

        private MLRegressionMetrics CrossValidationResultsToMetrics(IReadOnlyList<CrossValidationResult<RegressionMetrics>> crossValidationResults)
        {
            var metrics = new MLRegressionMetrics();
            foreach (var fold in crossValidationResults)
            {
                metrics.MeanAbsoluteError += fold.Metrics.MeanAbsoluteError;
                metrics.LossFunction += fold.Metrics.LossFunction;
                metrics.MeanSquaredError += fold.Metrics.MeanSquaredError;
                metrics.RootMeanSquaredError += fold.Metrics.RootMeanSquaredError;
                metrics.RSquared += fold.Metrics.RSquared;
            }

            metrics.MeanAbsoluteError /= crossValidationResults.Count();
            metrics.LossFunction /= crossValidationResults.Count();
            metrics.MeanSquaredError /= crossValidationResults.Count();
            metrics.RootMeanSquaredError /= crossValidationResults.Count();
            metrics.RSquared /= crossValidationResults.Count();

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

        private float GetLabelFromStockData(StockData stockData)
        {
            switch (Parameters.RegressionPoint)
            {
                case RegressionKernelEstimationLinearModelParameters.StockRegressionPointMethod.PreviousCloseToHigh:
                    return (float)stockData.PercentChangeHighToPreviousClose;
                case RegressionKernelEstimationLinearModelParameters.StockRegressionPointMethod.PreviousCloseToLow:
                    return (float)stockData.PercentChangeLowToPreviousClose;
                case RegressionKernelEstimationLinearModelParameters.StockRegressionPointMethod.PreviousCloseToClose:
                default:
                    return (float)stockData.PercentChangeFromPreviousClose;

            }
        }
    }
}
