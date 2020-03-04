using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Transforms;
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
        }

    }

    public class MLStockBinaryFastTreeModel : IBinaryStockPredictionModel<FastTreeBinaryModelParameters>
    {
        private IFeatureDatasetService _featureDatasetService;
        private MLContext _mLContext;
        private int _seed;
        private ITransformer _dataNormalizer;
        private ITransformer _featureSelector;
        private ITransformer _predictor;
        private ITransformer _completeModel;

        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }
        public FastTreeBinaryModelParameters Parameters { get; set; }

        public MLStockBinaryFastTreeModel(IFeatureDatasetService featureDatasetService, string symbol)
        {
            StockSymbol = symbol;
            _featureDatasetService = featureDatasetService;
            _seed = 27;
            _mLContext = new MLContext(_seed);
            Parameters = new FastTreeBinaryModelParameters();

        }

        public Result Load(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<StockPrediction> Predict(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Result<StockPrediction> PredictLatest()
        {
            throw new NotImplementedException();
        }

        public Result Save(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<BinaryClassificationMetrics> Train(DateTime startDate, DateTime endDate, double testFraction)
        {
            var dataset = _featureDatasetService.GetTrainingData(StockSymbol, startDate, endDate);
            if (dataset.IsFailure)
            {
                return Result.Failure<BinaryClassificationMetrics>(dataset.Error);
            }

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            int featureDimension = dataset.Value.FirstOrDefault().Input.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            //Load the data into a view
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Value.Select(x =>
                new StockRiseDataFeature(x.Input.Data, x.Output.PercentDayChange >= 0, (float)x.Output.PercentDayChange)), definedSchema);

            _dataNormalizer = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true).Fit(dataViewData);
            var normalizedData = _dataNormalizer.Transform(dataViewData);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(normalizedData, testFraction, seed: _seed);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = normalizedData;
            }

            _completeModel = GetBestModel(trainData, -4.0f, 4.0f);

            if (testData != null)
            {
                var positivePrediction = _completeModel.Transform(testData);
                var posResults = _mLContext.BinaryClassification.Evaluate(positivePrediction);

                return Result.Ok<BinaryClassificationMetrics>(posResults);
            }

            return Result.Ok<BinaryClassificationMetrics>(null);

        }

        private double EvaluateUpperPredictionAccuracy(ITransformer model, IDataView testData)
        {
            var predictions = model.Transform(testData);

            var probabilities = predictions.GetColumn<float>("Probability").ToArray();
            var labels = testData.GetColumn<bool>("Label").ToArray();

            float pThreshold = probabilities.Where(x => x > 0.5f).Average();
            double upper = 0.0, total = 0.0;
            for (int i = 0; i < probabilities.Length; ++i)
            {
                if (probabilities[i] > pThreshold)
                {
                    total++;
                    if (labels[i])
                    {
                        upper++;
                    }
                }
            }
            upper /= total;

            return upper;
        }

        private ITransformer GetBestModel(IDataView dataViewData, float lowStdev, float highStdev)
        {
            var featureSelector = new BinaryClassificationFeatureSelectorEstimator(_mLContext,
                lowerStdev: lowStdev,
                upperStdev: highStdev,
                inclusive: true)
                .Fit(dataViewData);
            //_featureSelector = _mLContext.Transforms.FeatureSelection.SelectFeaturesBasedOnMutualInformation(
            //   "Features", "Features", "Label", featureDimension / 10).Fit(normalizedData);
            var featureSelectedData = featureSelector.Transform(dataViewData);

            int crossValidations = Parameters.NumCrossValidations;
            int iterations = Parameters.NumIterations;
            int numberOfTrees = Parameters.NumOfTrees;
            int numberOfLeaves = Parameters.NumOfLeaves;

            var trainer = _mLContext.Transforms.FeatureSelection.SelectFeaturesBasedOnMutualInformation(
                "Features", slotsInOutput: Parameters.PcaRank * 2)
                .Append(_mLContext.Transforms.ProjectToPrincipalComponents(outputColumnName: "Features",
                rank: Parameters.PcaRank, overSampling: Parameters.PcaRank))
            //var trainer = _mLContext.Transforms.ApproximatedKernelMap(outputColumnName: "Features",
            //    rank: Parameters.PcaRank)
                    .Append(_mLContext.BinaryClassification.Trainers.FastTree(
                        numberOfLeaves: numberOfLeaves,
                        numberOfTrees: numberOfTrees,
                        minimumExampleCountPerLeaf: Parameters.MinNumOfLeaves));

            CrossValidationResult<CalibratedBinaryClassificationMetrics> bestCvResult = null;
            for (int it = 0; it < iterations; ++it)
            {
                var cvResults = _mLContext.BinaryClassification.CrossValidate(featureSelectedData, trainer, crossValidations);
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

            var predictor = bestCvResult.Model;

            var model = featureSelector
                .Append(predictor);

            return model;
        }
    }
}
