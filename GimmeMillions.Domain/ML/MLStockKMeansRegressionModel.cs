using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class MLStockKMeansRegressionModel : IStockPredictionModel
    {
        private IFeatureDatasetService _featureDatasetService;
        private MLContext _mLContext;
        private int _seed;
        public string StockSymbol { get; private set; }
        public bool IsTrained { get; private set; }
        public MLStockKMeansRegressionModel(IFeatureDatasetService featureDatasetService, string symbol)
        {
            StockSymbol = symbol;
            _featureDatasetService = featureDatasetService;
            _seed = 27;
            _mLContext = new MLContext();

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

            var averagePercentDayChange = dataset.Value.Select(x => x.Output.PercentDayChange).Average();

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
                new StockDailyValueDataFeature(x.Input.Data,
                    (float)x.Output.PercentDayChange)), definedSchema);

            //var normalizedData = _mLContext.Transforms.NormalizeMeanVariance("Features")
            //    //.Append(_mLContext.Transforms.FeatureSelection.SelectFeaturesBasedOnMutualInformation("Features"))
            //    .Fit(dataViewData)
            //    .Transform(dataViewData);

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

            //var featureAdder = _mLContext.Transforms.Concatenate("Features", "Features", "Label").Fit(trainData);

            //var kMeans = _mLContext.Clustering.Trainers.KMeans(new KMeansTrainer.Options()
            //    {
            //        NumberOfClusters = dataset.Value.Count()/8,
            //        OptimizationTolerance = 1e-6f,
            //        NumberOfThreads = 1
            //    });
            //var model = kMeans.Fit(featureAdder.Transform(trainData));
            //VBuffer<float>[] centroids = default;
            //var modelParams = model.Model;
            //modelParams.GetClusterCentroids(ref centroids, out int numClusters);
            //for(int k = 0; k < centroids.Length; ++k)
            //{
            //    var c = centroids[k].GetValues().ToArray();

            //}
            var clustersDataView = _mLContext.Transforms.Concatenate("Features", "Features", "Label").Fit(trainData).Transform(trainData);
            var clusters = clustersDataView.GetColumn<float[]>("Features").ToArray();

            VBuffer<float>[] centroids = new VBuffer<float>[clusters.Length];
            for(int i = 0; i < clusters.Length; ++i)
            {
                centroids[i] = new VBuffer<float>(clusters[i].Length, clusters[i]);
            }

            if (testData != null)
            {
                var features = testData.GetColumn<float[]>("Features").ToArray();
                var labels = testData.GetColumn<float>("Label").ToArray();

                var accuracy = 0.0f;
                var error = 0.0f;
                var predictions = new float[labels.Length];
                for(int i = 0; i < features.Length; ++i)
                {
                    var prediction = GetPrediction(features[i], centroids);
                    if((prediction > 0.0f && labels[i] > 0.0f) || (prediction < 0.0f && labels[i] < 0.0f))
                    {
                        accuracy++;
                    }
                    error += Math.Abs(labels[i] - prediction);
                    predictions[i] = prediction;
                }

                accuracy /= features.Length;
                error /= features.Length;

                double upperThreshold = predictions.Where(x => x > 0).Average();
                double accuracyAboveThreshold = 0.0, totalAboveThreshold = 0.0;
                for (int i = 0; i < features.Length; ++i)
                {
                    if(predictions[i] > upperThreshold)
                    {
                        totalAboveThreshold++;
                        if (labels[i] > 0)
                        {
                            accuracyAboveThreshold++;
                        }
                    }
                }

                accuracyAboveThreshold /= totalAboveThreshold;

            }

            return Result.Ok(new TrainingResult());
        }

        private float CalculateDistance(float[] v1, float[] v2)
        {
            float sum = 0.0f;
            for(int i = 0; i < v1.Length && i < v2.Length; ++i)
            {
                sum += (float)Math.Pow(v1[i] - v2[i], 2.0);
            }
            return (float)Math.Sqrt(sum);
        }

        private float GetPrediction(float[] v, VBuffer<float>[] centroids)
        {
            float prediction = 0.0f;
            float minDistance = float.MaxValue;

            var labels = new float[centroids.Length];
            var distances = new float[centroids.Length];
            for (int i = 0; i < centroids.Length; ++i)
            {
                var c = centroids[i].GetValues().ToArray();
                labels[i] = c[c.Length - 1];
                var d = CalculateDistance(v, c);
                distances[i] = d;
                if (d < minDistance)
                {
                    minDistance = d;
                }
            }

            for (int i = 0; i < centroids.Length; ++i)
            {
                if (distances[i] < minDistance * 1.5f)
                {
                    float factor = minDistance / distances[i];
                    prediction += labels[i] * factor;
                }
            }

            return prediction;
        }
    }
}
