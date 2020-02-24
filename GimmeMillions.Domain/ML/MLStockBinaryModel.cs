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
        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public MLStockBinaryModel(IFeatureDatasetService featureDatasetService, string symbol)
        {
            StockSymbol = symbol;
            _featureDatasetService = featureDatasetService;
            int seed = 27;
            _mLContext = new MLContext(seed);

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
            var normalize = _mLContext.Transforms.NormalizeMeanVariance("Features",
                useCdf: true);
            var normalizeTransform = normalize.Fit(dataViewData);
            var transformedData = normalizeTransform.Transform(dataViewData);

            var pcaTransform = _mLContext.Transforms.ApproximatedKernelMap("Features",
                rank: 2000,
                generator: new GaussianKernel(gamma: 1.5f)).Fit(transformedData);
            var pcaTransformedData = pcaTransform.Transform(transformedData);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if (testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(pcaTransformedData, testFraction);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = pcaTransformedData;
            }

            var trainer = _mLContext.BinaryClassification.Trainers.FastTree(
                numberOfLeaves: 10,
                numberOfTrees: 40,
                minimumExampleCountPerLeaf: 1);
            //var trainer = _mLContext.BinaryClassification.Trainers.FastForest(
            //    numberOfLeaves: 50,
            //    numberOfTrees: 500,
            //    minimumExampleCountPerLeaf: 10);
            //var trainer = _mLContext.BinaryClassification.Trainers.LightGbm(
            //    numberOfLeaves: 500,
            //    minimumExampleCountPerLeaf: 10);

            //var trainer = _mLContext.BinaryClassification.Trainers.LbfgsLogisticRegression();
            //var trainer = _mLContext.BinaryClassification.Trainers.AveragedPerceptron();
            //var trainer = _mLContext.BinaryClassification.Trainers.SdcaLogisticRegression();
            //var trainer = _mLContext.BinaryClassification.Trainers.SymbolicSgdLogisticRegression();
            var trainedModel = trainer.Fit(trainData);

            var cvResults = _mLContext.BinaryClassification.CrossValidate(trainData, trainer, 5);
            var averageAccuracy = cvResults.Sum(x => x.Metrics.Accuracy) / (double)cvResults.Count();

            var trainingResult = new TrainingResult();
            var trainDataPredictions = trainedModel.Transform(trainData);
            bool[] predictionColumn = trainDataPredictions.GetColumn<bool>("PredictedLabel").ToArray();
            bool[] labelColumn = trainDataPredictions.GetColumn<bool>("Label").ToArray();
            float[] probabilityColumn = trainDataPredictions.GetColumn<float>("Probability").ToArray();
            for (int i = 0; i < predictionColumn.Length; ++i)
            {
                if ((predictionColumn[i] && labelColumn[i]) || (!predictionColumn[i] && !labelColumn[i]))
                {
                    trainingResult.TrainingAccuracy++;
                }
            }

            trainingResult.TrainingAccuracy /= (double)predictionColumn.Length;

            if (testData != null)
            {
                var testDataPredictions = trainedModel.Transform(testData);
                predictionColumn = testDataPredictions.GetColumn<bool>("PredictedLabel").ToArray();
                labelColumn = testDataPredictions.GetColumn<bool>("Label").ToArray();
                probabilityColumn = testDataPredictions.GetColumn<float>("Probability").ToArray();
                var valueColumn = testDataPredictions.GetColumn<float>("Value").ToArray();
                for (int i = 0; i < predictionColumn.Length; ++i)
                {
                    if ((predictionColumn[i] && labelColumn[i]) || (!predictionColumn[i] && !labelColumn[i]))
                    {
                        trainingResult.ValidationAccuracy++;
                    }
                }

                trainingResult.ValidationAccuracy /= (double)predictionColumn.Length;
            }

            var probabilitiesToWin = probabilityColumn.Where(x => x > 0.5f);
            var averageProbToWin = probabilitiesToWin.Sum() / probabilitiesToWin.Count();
            var stdevProbToWin = Math.Sqrt(probabilitiesToWin.Sum(x => (x - averageProbToWin) * (x - averageProbToWin))
                                        / probabilitiesToWin.Count());

            return Result.Ok(trainingResult);

        }
    }
}
