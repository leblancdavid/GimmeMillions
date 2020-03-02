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

namespace GimmeMillions.Domain.ML
{
    public class MLStockBinaryModel : IStockPredictionModel
    {
        private IFeatureDatasetService _featureDatasetService;
        private MLContext _mLContext;
        private int _seed;
        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public MLStockBinaryModel(IFeatureDatasetService featureDatasetService, string symbol)
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

        public Result<TrainingResult> Train(DateTime startDate, DateTime endDate, double testFraction)
        {
            var dataset = _featureDatasetService.GetTrainingData(StockSymbol, startDate, endDate);
            if (dataset.IsFailure)
            {
                return Result.Failure<TrainingResult>(dataset.Error);
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

            var normalizedData = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true)
                .Append(new BinaryClassificationFeatureSelectorEstimator(_mLContext, lowerStdev: -3.5f, upperStdev: -1.0f, inclusive: true))
                .Fit(dataViewData)
                .Transform(dataViewData);

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

            int crossValidations = 10;
            int iterations = 10;
            int numberOfTrees = dataset.Value.Count() / 20;
            int numberOfLeaves = numberOfTrees / 5;


            var trainer = _mLContext.Transforms.ProjectToPrincipalComponents(outputColumnName: "Features", rank: 600, overSampling: 600)
            //var trainer = _mLContext.Transforms.ApproximatedKernelMap(outputColumnName: "Features", rank: numberOfTrees)
                    .Append(_mLContext.BinaryClassification.Trainers.FastTree(
                        numberOfLeaves: numberOfLeaves,
                        numberOfTrees: numberOfTrees,
                        minimumExampleCountPerLeaf: 1));
            //var trainer = _mLContext.BinaryClassification.Trainers.FastTree(
            //            numberOfLeaves: numberOfLeaves,
            //            numberOfTrees: numberOfTrees,
            //            minimumExampleCountPerLeaf: 1);

            for (int i = 0; i < iterations; ++i)
            {
                var cvResults = _mLContext.BinaryClassification.CrossValidate(trainData, trainer, crossValidations);

                if (testData != null)
                {
                    //var testResults = new List<BinaryClassificationMetrics>();
                    double upperAverage = 0.0;
                    var upperPredictionTestAccuracy = new List<double>();
                    //var upperPredictionValidationAccuracy = new List<double>();
                    foreach (var cv in cvResults)
                    {
                        upperPredictionTestAccuracy.Add(EvaluateUpperPredictionAccuracy(cv.Model, testData));
                        //upperPredictionValidationAccuracy.Add(EvaluateUpperPredictionAccuracy(cv.Model, ));

                    }
                    upperAverage = upperPredictionTestAccuracy.Average();
                }
            }
            

            return Result.Ok(new TrainingResult());

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
