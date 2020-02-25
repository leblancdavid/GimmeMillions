using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
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

            //Normalize it

            //var pcaTransform = _mLContext.Transforms.ApproximatedKernelMap("Features",
            //    rank: 1000,
            //    generator: new GaussianKernel(gamma: 1.5f)).Fit(transformedData);
            //var pcaTransformedData = pcaTransform.Transform(transformedData);


            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(dataViewData, testFraction, seed: _seed);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = dataViewData;
            }

            //var trainer = _mLContext.BinaryClassification.Trainers.FastTree(
            //    numberOfLeaves: 5,
            //    numberOfTrees: 20,
            //    minimumExampleCountPerLeaf: 1);
            //var trainer = _mLContext.BinaryClassification.Trainers.FastForest(
            //    numberOfLeaves: 20,
            //    numberOfTrees: 100,
            ////    minimumExampleCountPerLeaf: 1);
            //var trainer = _mLContext.BinaryClassification.Trainers.LightGbm(
            //    numberOfLeaves: 100,
            //    minimumExampleCountPerLeaf: 1);

            //var trainer = _mLContext.BinaryClassification.Trainers.LbfgsLogisticRegression();
            //var trainer = _mLContext.BinaryClassification.Trainers.SdcaLogisticRegression();
            //var trainer = _mLContext.BinaryClassification.Trainers.SymbolicSgdLogisticRegression();

            //var trainedModel = trainer.Fit(trainData);


            //var trainer = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: false)
            //    .Append(_mLContext.Transforms.ProjectToPrincipalComponents("Features", rank: 10))
            //    .Append(_mLContext.BinaryClassification.Trainers.AveragedPerceptron());

            var trainer = _mLContext.Transforms.ApproximatedKernelMap("Features", rank: 2000, generator: new GaussianKernel(gamma: 1.5f))
            .Append(_mLContext.BinaryClassification.Trainers.FastTree(
                numberOfLeaves: 4,
                numberOfTrees: 20,
                minimumExampleCountPerLeaf: 1));
            //.Append(_mLContext.BinaryClassification.Trainers.LightGbm(
            //    numberOfLeaves: 20,
            //    minimumExampleCountPerLeaf: 1));

            //var cvResults = _mLContext.BinaryClassification.CrossValidateNonCalibrated(trainData, trainer, 5, seed: _seed);
            var models = new List<ITransformer>();
            int numModels = 50;
            for(int i = 0; i < numModels; ++i)
            {
                models.Add(trainer.Fit(trainData));
            }


            //var trainingMetrics = new List<TrainingMetrics<BinaryClassificationMetrics>>();
            //foreach (var cv in cvResults)
            //{
            //    var m = new TrainingMetrics<BinaryClassificationMetrics>(cv.Metrics);
            //    var predictionData = cv.Model.Transform(cv.ScoredHoldOutSet);
            //    m.ComputeStatistics(predictionData, 0.0, 6.0);

            //    trainingMetrics.Add(m);
            //}

            var testMetrics = new List<TrainingMetrics<BinaryClassificationMetrics>>();
            if (testData != null)
            {
                var labelsColumn = testData.GetColumn<bool>("Label").ToArray();
                double accuracy = 0.0;
                var averageP = new float[labelsColumn.Length];
                foreach (var cv in models)
                {
                    //var m = new TrainingMetrics<BinaryClassificationMetrics>(cv.Metrics);
                    //var predictionData = cv.Model.Transform(testData);
                    //m.ComputeStatistics(predictionData, 0.0, 6.0);

                    //testMetrics.Add(m);
                    var predictionData = cv.Transform(testData);

                    var l = predictionData.GetColumn<bool>("PredictedLabel").ToArray();
                    var p = predictionData.GetColumn<float>("Probability").ToArray();
                    for (int i = 0; i < l.Length; ++i)
                    {
                        if(l[i] == labelsColumn[i])
                            accuracy++;

                        averageP[i] += p[i];
                    }
                }

                for (int i = 0; i < averageP.Length; ++i)
                {
                    averageP[i] /= models.Count();
                }
                accuracy /= (double)labelsColumn.Length * models.Count();


            }


            return Result.Ok(new TrainingResult());

        }
    }
}
