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
        public float LowerStdDev { get; set; }
        public float UpperStdDev { get; set; }
        public int NumCrossValidations { get; set; }
        public int NumIterations { get; set; }
        public int NumOfTrees { get; set; }
        public int NumOfLeaves { get; set; }
        public int MinNumOfLeaves { get; set; }
        public int PcaRank { get; set; }
        public int FeatureSelectionRank { get; set; }
        public FastTreeBinaryModelParameters()
        {
            LowerStdDev = -4.0f;
            UpperStdDev = -1.5f;
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

        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }
        public FastTreeBinaryModelParameters Parameters { get; set; }

        public MLStockBinaryFastForestModel(string symbol)
        {
            StockSymbol = symbol;
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
            int featureDimension = dataset.FirstOrDefault().Input.Length;
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
    }
}
