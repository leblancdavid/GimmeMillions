using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Transforms;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Transforms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.TrainCatalogBase;

namespace GimmeMillions.Domain.ML.Binary
{
    public class RandomFastTreeBinaryModelParameters
    {
        public int NumCrossValidations { get; set; }
        public int NumIterations { get; set; }
        public int NumOfTrees { get; set; }
        public int NumOfLeaves { get; set; }
        public int MinNumOfLeaves { get; set; }
        public int PcaRank { get; set; }
        public int FeatureSelectionRank { get; set; }
        public RandomFastTreeBinaryModelParameters()
        {
            NumCrossValidations = 10;
            NumIterations = 10;
            NumOfTrees = 100;
            NumOfLeaves = 20;
            MinNumOfLeaves = 1;

            PcaRank = 20;
            FeatureSelectionRank = 100;
        }

    }

    public class MLStockRandomFeatureFastTreeModel : IBinaryStockPredictionModel<RandomFastTreeBinaryModelParameters>
    {
        private MLContext _mLContext;
        private int _seed;
        private ITransformer _dataNormalizer;
        private FeatureFilterTransform _featureSelector;
        private ITransformer _predictor;
        private ITransformer _model;

        private DataViewSchema _dataSchema;
        public RandomFastTreeBinaryModelParameters Parameters { get; set; }
        public BinaryPredictionModelMetadata<RandomFastTreeBinaryModelParameters> Metadata { get; private set; }

        public MLStockRandomFeatureFastTreeModel()
        {
            Metadata = new BinaryPredictionModelMetadata<RandomFastTreeBinaryModelParameters>();
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new RandomFastTreeBinaryModelParameters();

        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            try
            {
                string directory = $"{pathToModel}/{encoding}";

                Metadata = JsonConvert.DeserializeObject<BinaryPredictionModelMetadata<RandomFastTreeBinaryModelParameters>>(
                    File.ReadAllText($"{ directory}/{symbol}-meta.json"));

                DataViewSchema schema = null;
                _dataNormalizer = _mLContext.Model.Load($"{directory}/{Metadata.StockSymbol}-normalizer.zip", out schema);
                var selectorLoad = FeatureFilterTransform.LoadFromFile($"{ directory}/{ Metadata.StockSymbol}-selector.json", _mLContext);
                if (selectorLoad.IsFailure)
                {
                    return Result.Failure(selectorLoad.Error);
                }
                _featureSelector = selectorLoad.Value;
                _predictor = _mLContext.Model.Load($"{directory}/{Metadata.StockSymbol}-predictor.zip", out schema);

                _model = _dataNormalizer.Append(_featureSelector).Append(_predictor);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to load the model: {ex.Message}");
            }
        }

        public Result<StockPrediction> Predict(FeatureVector input)
        {
            //Load the data into a view
            var inputDataView = _mLContext.Data.LoadFromEnumerable(
                new List<StockRiseDataFeature>()
                {
                    new StockRiseDataFeature(input.Data, false, 0.0f,
                    (int)input.Date.DayOfWeek / 7.0f, input.Date.Month / 12.0f)
                },
                GetSchemaDefinition(input));

            var prediction = _model.Transform(inputDataView);

            var score = prediction.GetColumn<float>("Score").ToArray();
            var predictedLabel = prediction.GetColumn<bool>("PredictedLabel").ToArray();
            //var probability = prediction.GetColumn<float>("Probability").ToArray();

            return Result.Ok(new StockPrediction()
            {
                Score = score[0],
                PredictedLabel = predictedLabel[0],
                //Probability = probability[0]
                Probability = predictedLabel[0] ? 1.0f : 0.0f
            });
        }

        public Result Save(string pathToModel)
        {
            try
            {
                if (!Metadata.IsTrained)
                {
                    return Result.Failure("Model has not been trained or loaded");
                }

                string directory = $"{pathToModel}/{Metadata.FeatureEncoding}";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText($"{directory}/{Metadata.StockSymbol}-meta.json", JsonConvert.SerializeObject(Metadata, Formatting.Indented));
                _mLContext.Model.Save(_dataNormalizer, _dataSchema, $"{directory}/{Metadata.StockSymbol}-normalizer.zip");
                _featureSelector.SaveToFile($"{ directory}/{ Metadata.StockSymbol}-selector.json");
                _mLContext.Model.Save(_predictor, _dataSchema, $"{directory}/{Metadata.StockSymbol}-predictor.zip");

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
                var dataSplit = _mLContext.Data.TrainTestSplit(datasetView, testFraction: 0.2);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = datasetView;
            }

            _dataSchema = trainData.Schema;
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;
            Metadata.StockSymbol = firstFeature.Output.Symbol;

            var filterFeatures = (FeatureFilterTransform)new FeatureFrequencyUsageFilterEstimator(_mLContext, rank: (int)(firstFeature.Input.Length * 0.5))
               .Fit(trainData);
            var filteredData = filterFeatures.Transform(trainData);
            var featureSelector = (FeatureFilterTransform)new MaxVarianceFeatureFilterEstimator(_mLContext, rank: Parameters.FeatureSelectionRank)
               .Fit(filteredData);
            var featureSelectedData = featureSelector.Transform(filteredData);

            var pipeline = _mLContext.Transforms.NormalizeSupervisedBinning("Features")
                    .Append(_mLContext.Transforms.Concatenate("Features", "Features", "DayOfTheWeek", "Month"))
                    .Append(_mLContext.Transforms.NormalizeMinMax("Features"))
                    //.Append(_mLContext.Transforms.Concatenate("Features", "Features", "DayOfTheWeek"))
                    //.Append(_mLContext.Transforms.NormalizeMinMax("Features"))
                    //.Append(_mLContext.BinaryClassification.Trainers.LinearSvm(numberOfIterations: 25));
                    .Append(_mLContext.BinaryClassification.Trainers.SdcaLogisticRegression());
                    //.Append(_mLContext.BinaryClassification.Trainers.AveragedPerceptron());
                    //.Append(_mLContext.BinaryClassification.Trainers.SymbolicSgdLogisticRegression());
                    //.Append(_mLContext.BinaryClassification.Trainers.LbfgsLogisticRegression());
                    //.Append(_mLContext.BinaryClassification.Trainers.FastTree(
                    //    numberOfLeaves: Parameters.NumOfLeaves,
                    //    numberOfTrees: Parameters.NumOfTrees,
                    //    minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves));
            //.Append(_mLContext.BinaryClassification.Trainers.FastForest(
            //    numberOfLeaves: numberOfLeaves,
            //    numberOfTrees: numberOfTrees,
            //    minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves));

            if (Parameters.NumCrossValidations > 1)
            {
                var cvResults = _mLContext.BinaryClassification.CrossValidate(featureSelectedData, pipeline, Parameters.NumCrossValidations);
                UpdateMetadata(cvResults);
            }
            
            var predictor = pipeline.Fit(featureSelectedData);
            //_model = featureSelector.Append(predictor);
            _model = filterFeatures.Append(featureSelector.Append(predictor));
            //_model = predictor;
            if (testData != null)
            {
                var testPredictions = _model.Transform(testData);
                var testResults = _mLContext.BinaryClassification.Evaluate(testPredictions);

                Metadata.TrainingResults = new ModelMetrics(testResults);
            }
            return Result.Ok<ModelMetrics>(Metadata.TrainingResults);
        }

        private void UpdateMetadata(IReadOnlyList<CrossValidationResult<CalibratedBinaryClassificationMetrics>> crossValidationResults)
        {
            Metadata.Parameters = Parameters;
            Metadata.TrainingResults = new ModelMetrics();

            float lowerCount = 0.0f, upperCount = 0.0f;
            Metadata.AverageLowerProbability = 0.0f;
            Metadata.AverageUpperProbability = 0.0f;
            Metadata.AverageLowerScore = 0.0f;
            Metadata.AverageUpperScore = 0.0f;
            foreach (var fold in crossValidationResults)
            {
                Metadata.TrainingResults.Accuracy += fold.Metrics.Accuracy;
                Metadata.TrainingResults.AreaUnderPrecisionRecallCurve += fold.Metrics.AreaUnderPrecisionRecallCurve;
                Metadata.TrainingResults.AreaUnderRocCurve += fold.Metrics.AreaUnderRocCurve;
                Metadata.TrainingResults.F1Score += fold.Metrics.F1Score;
                Metadata.TrainingResults.NegativePrecision += fold.Metrics.NegativePrecision;
                Metadata.TrainingResults.NegativeRecall += fold.Metrics.NegativeRecall;
                Metadata.TrainingResults.PositivePrecision += fold.Metrics.PositivePrecision;
                Metadata.TrainingResults.PositiveRecall += fold.Metrics.PositiveRecall;

                var probabilities = fold.ScoredHoldOutSet.GetColumn<float>("Probability").ToArray();
                var scores = fold.ScoredHoldOutSet.GetColumn<float>("Score").ToArray();
                for (int i = 0; i < probabilities.Length; ++i)
                {
                    if (probabilities[i] >= 0.5f)
                    {
                        Metadata.AverageUpperProbability += probabilities[i];
                        Metadata.AverageUpperScore += scores[i];
                        upperCount++;
                    }
                    else
                    {
                        Metadata.AverageLowerProbability += probabilities[i];
                        Metadata.AverageLowerScore += scores[i];
                        lowerCount++;
                    }
                }

            }

            Metadata.TrainingResults.Accuracy /= crossValidationResults.Count();
            Metadata.TrainingResults.AreaUnderPrecisionRecallCurve /= crossValidationResults.Count();
            Metadata.TrainingResults.AreaUnderRocCurve /= crossValidationResults.Count();
            Metadata.TrainingResults.F1Score /= crossValidationResults.Count();
            Metadata.TrainingResults.NegativePrecision /= crossValidationResults.Count();
            Metadata.TrainingResults.NegativeRecall /= crossValidationResults.Count();
            Metadata.TrainingResults.PositivePrecision /= crossValidationResults.Count();
            Metadata.TrainingResults.PositiveRecall /= crossValidationResults.Count();

            if (lowerCount > 0)
            {
                Metadata.AverageLowerProbability /= lowerCount;
                Metadata.AverageLowerScore /= lowerCount;
            }
            else
            {
                Metadata.AverageLowerProbability = 0.0f;
                Metadata.AverageLowerScore = 0.0f;
            }

            if (upperCount > 0)
            {
                Metadata.AverageUpperProbability /= upperCount;
                Metadata.AverageUpperScore /= upperCount;
            }
            else
            {
                Metadata.AverageUpperProbability = 0.0f;
                Metadata.AverageUpperScore = 0.0f;
            }
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
