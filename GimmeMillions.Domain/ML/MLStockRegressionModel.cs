using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Trainers.LightGbm;
using Microsoft.ML.Transforms;

namespace GimmeMillions.Domain.ML
{
    public class MLStockRegressionModel : IStockPredictionModel
    {
        private IFeatureDatasetService _featureDatasetService;
        private MLContext _mLContext;
        private int _seed;
        public string StockSymbol { get; private set; }

        public bool IsTrained { get; private set; }

        public MLStockRegressionModel(IFeatureDatasetService featureDatasetService, string symbol)
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
            if(dataset.IsFailure)
            {
                return Result.Failure<TrainingResult>(dataset.Error);
            }

            // The feature dimension (typically this will be the Count of the array 
            // of the features vector known at runtime).
            int featureDimension = dataset.Value.FirstOrDefault().Input.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockDailyValueDataFeature));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);

            //Load the data into a view
            var dataViewData = _mLContext.Data.LoadFromEnumerable(
                dataset.Value.Select(x => 
                new StockDailyValueDataFeature(x.Input.Data, (float)x.Output.PercentDayChange)), definedSchema);

            //Split data into training and testing
            IDataView trainData = null, testData = null;
            if(testFraction > 0.0)
            {
                var dataSplit = _mLContext.Data.TrainTestSplit(dataViewData, testFraction, seed: _seed);
                trainData = dataSplit.TrainSet;
                testData = dataSplit.TestSet;
            }
            else
            {
                trainData = dataViewData;
            }

            int numberOfTrees = dataset.Value.Count() / 10;
            int numberOfLeaves = numberOfTrees / 5;
            //Normalize it
            var trainer = _mLContext.Transforms.NormalizeMeanVariance("Features", useCdf: true)
                //.Append(_mLContext.Transforms.ApproximatedKernelMap("Features",
                //    rank: 10000,
                //    useCosAndSinBases: true))
                .Append(_mLContext.Transforms.FeatureSelection.SelectFeaturesBasedOnMutualInformation(
                    outputColumnName: "Features",
                    slotsInOutput: 50, numberOfBins: 8))
                .Append(_mLContext.Regression.Trainers.FastForest(
                    numberOfLeaves: numberOfLeaves,
                    numberOfTrees: numberOfTrees,
                    minimumExampleCountPerLeaf: 1));
            //.Append(_mLContext.Regression.Trainers.FastTree(
            //    numberOfLeaves: 10,
            //    numberOfTrees: 50,
            //    minimumExampleCountPerLeaf: 1));
            //.Append(_mLContext.Regression.Trainers.OnlineGradientDescent(numberOfIterations: 10));
            //.Append(_mLContext.Regression.Trainers.Sdca());
            //var trainer = _mLContext.Regression.Trainers.LightGbm(
            //    numberOfLeaves: 1000,
            //    minimumExampleCountPerLeaf: 0);

            //var cvResults = _mLContext.Regression.CrossValidate(trainData, trainer, 5);
            var models = new List<ITransformer>();
            int numModels = 10;
            for (int i = 0; i < numModels; ++i)
            {
                models.Add(trainer.Fit(trainData));
            }

            if (testData != null)
            {
                var labelsColumn = testData.GetColumn<float>("Label").ToArray();
                double accuracy = 0.0;
                var posP = new float[labelsColumn.Length];
                var negP = new float[labelsColumn.Length];
                var probability = new float[labelsColumn.Length];
                foreach (var model in models)
                {
                    var predictionData = model.Transform(testData);
                    var p = predictionData.GetColumn<float>("Score").ToArray();           
                    for (int i = 0; i < p.Length; ++i)
                    {
                        if ((p[i] > 0.0f && labelsColumn[i] > 0.0f) || (p[i] <= 0.0f && labelsColumn[i] <= 0.0f))
                            accuracy++;

                        if (p[i] > 0.0f)
                        {
                            posP[i] += p[i];
                        }
                        else
                        {
                            negP[i] += p[i];
                        }
                    }
                }

                for (int i = 0; i < probability.Length; ++i)
                {
                    probability[i] = posP[i] / (posP[i] - negP[i]);
                }

                accuracy /= (double)labelsColumn.Length * models.Count();


            }

            return Result.Ok(new TrainingResult());

        }
    }
}
