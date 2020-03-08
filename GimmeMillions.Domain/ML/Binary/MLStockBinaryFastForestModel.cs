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
    public class FastTreeBinaryModelParameters
    {
        public int NumCrossValidations { get; set; }
        public int NumIterations { get; set; }
        public int NumOfTrees { get; set; }
        public int NumOfLeaves { get; set; }
        public int MinNumOfLeaves { get; set; }
        public int PcaRank { get; set; }
        public int FeatureSelectionRank { get; set; }
        public FastTreeBinaryModelParameters()
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

    public class MLStockBinaryFastForestModel : IBinaryStockPredictionModel<FastTreeBinaryModelParameters>
    {
        private MLContext _mLContext;
        private int _seed;
        private ITransformer _dataNormalizer;
        private FeatureFilterTransform _featureSelector;
        private ITransformer _predictor;
        private ITransformer _model;

        private DataViewSchema _dataSchema;
        public FastTreeBinaryModelParameters Parameters { get; set; }
        public BinaryPredictionModelMetadata<FastTreeBinaryModelParameters> Metadata { get; private set; }

        public MLStockBinaryFastForestModel()
        {
            Metadata = new BinaryPredictionModelMetadata<FastTreeBinaryModelParameters>();
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new FastTreeBinaryModelParameters();

        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            try
            {
                string directory = $"{pathToModel}/{encoding}";

                Metadata = JsonConvert.DeserializeObject<BinaryPredictionModelMetadata<FastTreeBinaryModelParameters>>(
                    File.ReadAllText($"{ directory}/{symbol}-meta.json"));

                DataViewSchema schema = null;
                _dataNormalizer = _mLContext.Model.Load($"{directory}/{Metadata.StockSymbol}-normalizer.zip", out schema);
                var selectorLoad = FeatureFilterTransform.LoadFromFile($"{ directory}/{ Metadata.StockSymbol}-selector.json", _mLContext);
                if(selectorLoad.IsFailure)
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
            catch(Exception ex)
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

            int numTrainingSamples = (int)(dataset.Count() * (1.0 - testFraction));
            var trainingDataset = dataset.Take(numTrainingSamples);
            var testDataset = dataset.Skip(numTrainingSamples);
           
            //Load the data into a view
            var trainingDataView = _mLContext.Data.LoadFromEnumerable(
                trainingDataset.Select(x =>
                new StockRiseDataFeature(
                    x.Input.Data, x.Output.PercentDayChange >= 0, 
                    (float)x.Output.PercentDayChange,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.Month / 12.0f)), 
                GetSchemaDefinition(firstFeature.Input));

            var testDataView = _mLContext.Data.LoadFromEnumerable(
                testDataset.Select(x =>
                new StockRiseDataFeature(
                    x.Input.Data, x.Output.PercentDayChange >= 0,
                    (float)x.Output.PercentDayChange,
                    (int)x.Input.Date.DayOfWeek / 7.0f, x.Input.Date.Month / 12.0f)),
                GetSchemaDefinition(firstFeature.Input));

            _dataSchema = trainingDataView.Schema; 
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;
            Metadata.StockSymbol = firstFeature.Output.Symbol;


            //_dataNormalizer = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true).Fit(trainingDataView);
            //var normalizedData = _dataNormalizer.Transform(trainingDataView);
            //var featureSelector = _mLContext.Transforms.FeatureSelection.SelectFeaturesBasedOnMutualInformation("Features", slotsInOutput: Parameters.FeatureSelectionRank)
            //    .Fit(trainingDataView);
            //var selectedFeaturesData = featureSelector.Transform(trainingDataView);

            _featureSelector = (FeatureFilterTransform)(new MaxVarianceFeatureFilterEstimator(_mLContext,
                rank: Parameters.FeatureSelectionRank))
                .Fit(trainingDataView);
            var selectedFeaturesData = _featureSelector.Transform(trainingDataView);

            _dataNormalizer = _mLContext.Transforms.NormalizeMinMax("Features")
                .Fit(selectedFeaturesData);
            var normalizedData = _dataNormalizer.Transform(selectedFeaturesData);


            //_featureSelector = (FeatureFilterTransform)(new MaxVarianceFeatureFilterEstimator(_mLContext,
            //    rank: (int)(firstFeature.Input.Length / 3))
            //    .Fit(normalizedData));
            //var selectedFeaturesData = _featureSelector.Transform(normalizedData);

            //var trainingResults = GetBestTrainingModel(trainData);
            //_predictor = trainingResults.Model;
            _predictor = TrainModel(normalizedData);
            _model = _featureSelector.Append(_dataNormalizer).Append(_predictor);
            // _model = _dataNormalizer.Append(_featureSelector).Append(_predictor);
            //_model = _featureSelector.Append(_predictor);
            //_model = _dataNormalizer.Append(_predictor);

            if (testDataset.Any())
            {
                var positivePrediction = _model.Transform(testDataView);
                var testViewScores = positivePrediction.GetColumn<float>("Score").ToArray();

                var testResults = _mLContext.BinaryClassification.EvaluateNonCalibrated(positivePrediction);

                var testSamplePredictions = new List<StockPrediction>();
                foreach(var testSample in testDataset)
                {
                    testSamplePredictions.Add(Predict(testSample.Input).Value);
                }

                Metadata.TrainingResults = new ModelMetrics(testResults);
                return Result.Ok<ModelMetrics>(new ModelMetrics(testResults));
            }

            var trainResults = _mLContext.BinaryClassification.Evaluate(_predictor.Transform(trainingDataView));
            Metadata.TrainingResults = new ModelMetrics(trainResults);
            return Result.Ok<ModelMetrics>(new ModelMetrics(trainResults));

        }

        private ITransformer TrainModel(IDataView dataViewData)
        {
            int numberOfTrees = Parameters.NumOfTrees;
            int numberOfLeaves = Parameters.NumOfLeaves;

            return _mLContext.Transforms.ProjectToPrincipalComponents("Features", rank: Parameters.PcaRank, overSampling: Parameters.PcaRank)
                .Append(_mLContext.Transforms.Concatenate("Features", "Features", "DayOfTheWeek", "Month"))
                .Append(_mLContext.BinaryClassification.Trainers.FastForest(numberOfLeaves: numberOfLeaves, numberOfTrees: numberOfTrees, minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves, featureColumnName: "Features")).Fit(dataViewData);

        }

        private CrossValidationResult<CalibratedBinaryClassificationMetrics> GetBestTrainingModel(IDataView dataViewData)
        {
            int crossValidations = Parameters.NumCrossValidations;
            int iterations = Parameters.NumIterations;
            int numberOfTrees = Parameters.NumOfTrees;
            int numberOfLeaves = Parameters.NumOfLeaves;

            var trainer = _mLContext.Transforms.FeatureSelection.SelectFeaturesBasedOnMutualInformation("Features", slotsInOutput: Parameters.FeatureSelectionRank)
            //.Append(_mLContext.Transforms.ProjectToPrincipalComponents(
            //    outputColumnName: "Features",
            //    rank: Parameters.PcaRank, overSampling: Parameters.PcaRank))
            .Append(_mLContext.BinaryClassification.Trainers.FastTree(
                featureColumnName: "Features",
                numberOfLeaves: numberOfLeaves,
                numberOfTrees: numberOfTrees,
                minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves));

            //var trainer = _mLContext.Transforms.ProjectToPrincipalComponents(
            //    outputColumnName: "Features",
            //    rank: Parameters.PcaRank, overSampling: Parameters.PcaRank)
            //.Append(_mLContext.BinaryClassification.Trainers.FastTree(
            //    featureColumnName: "Features",
            //    numberOfLeaves: numberOfLeaves,
            //    numberOfTrees: numberOfTrees,
            //    minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves));

            CrossValidationResult<CalibratedBinaryClassificationMetrics> bestCvResult = null;
            //CrossValidationResult<BinaryClassificationMetrics> bestCvResult = null;
            for (int it = 0; it < iterations; ++it)
            {
                var cvResults = _mLContext.BinaryClassification.CrossValidate(dataViewData, trainer, crossValidations);
                //var cvResults = _mLContext.BinaryClassification.CrossValidateNonCalibrated(dataViewData, trainer, crossValidations);

                if (bestCvResult == null)
                    bestCvResult = cvResults.FirstOrDefault();

                foreach (var cv in cvResults)
                {
                    if (cv.Metrics.AreaUnderPrecisionRecallCurve > bestCvResult.Metrics.AreaUnderPrecisionRecallCurve)
                    {
                        bestCvResult = cv;
                    }
                }
            }

            UpdateMetadata(bestCvResult);
            return bestCvResult;
        }

        private void UpdateMetadata(CrossValidationResult<CalibratedBinaryClassificationMetrics> crossValidationResult)
        {
            Metadata.Parameters = Parameters;
            Metadata.TrainingResults = new ModelMetrics(crossValidationResult.Metrics);

            //var predictedSet = crossValidationResult.Model.Transform(crossValidationResult.ScoredHoldOutSet);
            var probabilities = crossValidationResult.ScoredHoldOutSet.GetColumn<float>("Score").ToArray();
            float lowerCount = 0.0f, upperCount = 0.0f;
            Metadata.AverageLowerProbability = 0.0f;
            Metadata.AverageUpperProbability = 0.0f;
            for (int i = 0; i < probabilities.Length; ++i)
            {
                if(probabilities[i] >= 0.0f)
                {
                    Metadata.AverageUpperProbability += probabilities[i];
                    upperCount++;
                }
                else
                {
                    Metadata.AverageLowerProbability += probabilities[i];
                    lowerCount++;
                }
            }

            if (lowerCount > 0)
                Metadata.AverageLowerProbability /= lowerCount;
            else
                Metadata.AverageLowerProbability = 0.0f;

            if (upperCount > 0)
                Metadata.AverageUpperProbability /= upperCount;
            else
                Metadata.AverageUpperProbability = 0.0f;
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
