﻿using CSharpFunctionalExtensions;
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
        public int PcaRank { get; set; }

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

            _featureSelector = new BinaryClassificationFeatureSelectorEstimator(_mLContext, lowerStdev: -3.5f, upperStdev: -1.0f, inclusive: true)
                .Fit(normalizedData);
            var featureSelectedData = _featureSelector.Transform(normalizedData);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(featureSelectedData, testFraction, seed: _seed);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = featureSelectedData;
            }

            int crossValidations = 10;
            int iterations = 10;
            int numberOfTrees = dataset.Value.Count() / 20;
            int numberOfLeaves = numberOfTrees / 5;

            var trainer = _mLContext.Transforms.ProjectToPrincipalComponents(outputColumnName: "Features", rank: 200, overSampling: 200)
                    .Append(_mLContext.BinaryClassification.Trainers.FastTree(
                        numberOfLeaves: numberOfLeaves,
                        numberOfTrees: numberOfTrees,
                        minimumExampleCountPerLeaf: 1));

            CrossValidationResult<CalibratedBinaryClassificationMetrics> bestCvResult = null;
            object progressLock = new object();
            Parallel.For(0, iterations, it =>
            {
                var cvResults = _mLContext.BinaryClassification.CrossValidate(trainData, trainer, crossValidations);
                lock (progressLock)
                {
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
            });

            _predictor = bestCvResult.Model;

            _completeModel = _dataNormalizer
                .Append(_featureSelector)
                .Append(_predictor);

            if (testData != null)
            {
                var predictions = _predictor.Transform(testData);
                return Result.Ok<BinaryClassificationMetrics>(_mLContext.BinaryClassification.Evaluate(predictions));
            }

            return Result.Ok<BinaryClassificationMetrics>(bestCvResult.Metrics);

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
    }
}
