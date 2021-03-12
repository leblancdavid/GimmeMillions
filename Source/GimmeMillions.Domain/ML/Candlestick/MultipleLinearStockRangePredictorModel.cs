using Accord.Statistics.Models.Regression.Linear;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.ML.Candlestick
{
    public class MultipleLinearStockRangePredictorModel : IStockRangePredictor
    {
        public bool IsTrained => false;

        public Result Load(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public StockRangePrediction Predict(FeatureVector Input)
        {
            throw new NotImplementedException();
        }

        public Result Save(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, 
            double testFraction, 
            ITrainingOutputMapper trainingOutputMapper)
        {
            if (!dataset.Any())
            {
                return Result.Failure<ModelMetrics>($"Training dataset is empty");
            }

            var firstFeature = dataset.FirstOrDefault();
            int trainingCount = (int)((double)dataset.Count() * (1.0 - testFraction));

            var trainingData = dataset.Take(trainingCount);
            var testData = dataset.Skip(trainingCount);

            var trainingInputs = trainingData.Select(x => x.Input.Data).ToArray();
            var signalOutputs = trainingData.Select(x => (double)trainingOutputMapper.GetOutputValue(x.Output)).ToArray();

            var ols = new OrdinaryLeastSquares()
            {
                UseIntercept = true
            };

            var signalModel = ols.Learn(trainingInputs, signalOutputs);

            var predictionResults = new List<(double PredictedSignal, double ActualSignal)>();
            foreach (var testSample in testData)
            {
                var prediction = signalModel.Transform(testSample.Input.Data);
                if (prediction > 0.80)
                    predictionResults.Add((prediction, trainingOutputMapper.GetOutputValue(testSample.Output)));

                if (prediction < 0.20)
                    predictionResults.Add((prediction, trainingOutputMapper.GetOutputValue(testSample.Output)));
            }

            predictionResults = predictionResults.OrderByDescending(x => x.PredictedSignal).ToList();
            var runningAccuracy = new List<double>();
            double correct = 0.0;
            int i = 0;
            foreach(var result in predictionResults)
            {
                if((result.PredictedSignal > 0.80 && result.ActualSignal > 0.80) || (result.PredictedSignal < 0.2 && result.ActualSignal < 0.2))
                    correct++;

                runningAccuracy.Add(correct / (double)(i + 1));
                i++;
            }

            return Result.Success<ModelMetrics>(null);
        }
    }
}
