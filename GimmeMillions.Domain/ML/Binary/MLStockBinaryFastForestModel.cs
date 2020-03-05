using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Transforms;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
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
        private ITransformer _featureSelector;
        private ITransformer _predictor;
        private PredictionEngine<StockRiseDataFeature, StockPrediction> _predictionEngine;

        public FastTreeBinaryModelParameters Parameters { get; set; }
        public BinaryPredictionModelMetadata<FastTreeBinaryModelParameters> Metadata { get; private set; }

        public MLStockBinaryFastForestModel()
        {
            Metadata = new BinaryPredictionModelMetadata<FastTreeBinaryModelParameters>();
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new FastTreeBinaryModelParameters();

        }

        public Result Load(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<StockPrediction> Predict(FeatureVector input)
        {
            throw new NotImplementedException();
        }

        public Result Save(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<BinaryClassificationMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            if (!dataset.Any())
            {
                return Result.Failure<BinaryClassificationMetrics>($"Training dataset is empty");
            }

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            var firstFeature = dataset.FirstOrDefault();
            Metadata.FeatureEncoding = firstFeature.Input.Encoding;
            Metadata.StockSymbol = firstFeature.Output.Symbol;

            int featureDimension = firstFeature.Input.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            //Load the data into a view
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Select(x =>
                new StockRiseDataFeature(x.Input.Data, x.Output.PercentDayChange >= 0, (float)x.Output.PercentDayChange)), definedSchema);

            _dataNormalizer = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true).Fit(dataViewData);
            var normalizedData = _dataNormalizer.Transform(dataViewData);

            _featureSelector = new MaxVarianceFeatureFilterEstimator(_mLContext,
                rank: Parameters.FeatureSelectionRank)
                .Fit(normalizedData);
            var selectedFeaturesData = _featureSelector.Transform(normalizedData);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(selectedFeaturesData, testFraction, seed: _seed);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = selectedFeaturesData;
            }

            var trainingResults = GetBestTrainingModel(trainData);
            _predictor = trainingResults.Model;

            var model = _dataNormalizer.Append(_featureSelector).Append(_predictor);

            UpdateMetadata(trainingResults);

            //_predictionEngine = _mLContext.Model.CreatePredictionEngine<StockRiseDataFeature, StockPrediction>(model);

            if (testData != null)
            {
                var positivePrediction = _predictor.Transform(testData);
                var testResults = _mLContext.BinaryClassification.EvaluateNonCalibrated(positivePrediction);

                return Result.Ok<BinaryClassificationMetrics>(testResults);
            }

            return Result.Ok<BinaryClassificationMetrics>(trainingResults.Metrics);

        }

        private CrossValidationResult<BinaryClassificationMetrics> GetBestTrainingModel(IDataView dataViewData)
        {
            int crossValidations = Parameters.NumCrossValidations;
            int iterations = Parameters.NumIterations;
            int numberOfTrees = Parameters.NumOfTrees;
            int numberOfLeaves = Parameters.NumOfLeaves;

            var trainer = _mLContext.Transforms.ProjectToPrincipalComponents(
                outputColumnName: "Features",
                rank: Parameters.PcaRank, overSampling: Parameters.PcaRank)
            .Append(_mLContext.BinaryClassification.Trainers.FastForest(
                featureColumnName: "Features",
                numberOfLeaves: numberOfLeaves,
                numberOfTrees: numberOfTrees,
                minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves));

            //CrossValidationResult<CalibratedBinaryClassificationMetrics> bestCvResult = null;
            CrossValidationResult<BinaryClassificationMetrics> bestCvResult = null;
            for (int it = 0; it < iterations; ++it)
            {
                //var cvResults = _mLContext.BinaryClassification.CrossValidate(dataViewData, trainer, crossValidations);
                var cvResults = _mLContext.BinaryClassification.CrossValidateNonCalibrated(dataViewData, trainer, crossValidations);

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

            return bestCvResult;
        }

        private void UpdateMetadata(CrossValidationResult<BinaryClassificationMetrics> crossValidationResult)
        {
            Metadata.Parameters = Parameters;
            Metadata.TrainingResults = crossValidationResult.Metrics;

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

    }
}
