using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Models.Regression.Linear;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.ML.Candlestick
{
    public class SVMStockRangePredictorModel : IStockRangePredictor
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
            double averageHigh = (double)trainingData.Average(x => x.Output.PercentChangeHighToPreviousClose);
            double stdevHigh = (double)trainingData.Average(x => Math.Abs(x.Output.PercentChangeHighToPreviousClose - (decimal)averageHigh));
            double averageLow = (double)trainingData.Average(x => x.Output.PercentChangeLowToPreviousClose);
            double stdevLow = (double)trainingData.Average(x => Math.Abs(x.Output.PercentChangeLowToPreviousClose - (decimal)averageLow));

            var signalOutputs = trainingData.Select(x => 
                new double[] {
                (double)trainingOutputMapper.GetOutputValue(x.Output),
                ToHighOutput(x.Output.PercentChangeHighToPreviousClose, averageHigh, 3.0),
                ToLowOutput(x.Output.PercentChangeLowToPreviousClose, averageLow, 3.0),
                }).ToArray();

            var network = new DeepBeliefNetwork(new BernoulliFunction(), 
                firstFeature.Input.Data.Length, 
                firstFeature.Input.Data.Length,
                //firstFeature.Input.Data.Length / 2,
                firstFeature.Input.Data.Length,
                //firstFeature.Input.Data.Length / 4,
                //firstFeature.Input.Data.Length / 8,
                //firstFeature.Input.Data.Length / 16,
                3);
            new GaussianWeights(network).Randomize();
            network.UpdateVisibleWeights();

            var teacher = new BackPropagationLearning(network);
            //var teacher = new DeepNeuralNetworkLearning(network)
            //{
            //    Algorithm = (ann, i) => new ParallelResilientBackpropagationLearning(ann),
            //    LayerIndex = network.Layers.Length - 1,
            //};

            //double[][] layerData = teacher.GetLayerInput(trainingInputs);

            int epochs = 10;
            int i = 0;
            for (i = 0; i < epochs; i++)
            {
                double error = teacher.RunEpoch(trainingInputs, signalOutputs);
            }



            var predictionResults = new List<(double PredictedSignal, double ActualSignal)>();
            foreach (var testSample in testData)
            {
                var prediction = network.Compute(testSample.Input.Data);
                //predictionResults.Add((prediction[0], trainingOutputMapper.GetOutputValue(testSample.Output)));

                //var prediction = signalModel.Score(testSample.Input.Data);
                if (prediction[0] > 0.8 && prediction[1] > prediction[2])
                    predictionResults.Add((prediction[0], trainingOutputMapper.GetOutputValue(testSample.Output)));

                if (prediction[0] < 0.2 && prediction[1] < prediction[2])
                    predictionResults.Add((prediction[0], trainingOutputMapper.GetOutputValue(testSample.Output)));
            }

            predictionResults = predictionResults.OrderByDescending(x => x.PredictedSignal).ToList();
            var runningAccuracy = new List<double>();
            double correct = 0.0;
            i = 0;
            foreach (var result in predictionResults)
            {
                if((result.PredictedSignal > 0.50 && result.ActualSignal > 0.50) || (result.PredictedSignal < 0.5 && result.ActualSignal < 0.5))
                    correct++;

                runningAccuracy.Add(correct / (double)(i + 1));
                i++;
            }

            return Result.Success<ModelMetrics>(null);
        }

        private double ToHighOutput(decimal percentChange, double averageHigh, double scaling)
        {
            if (percentChange < 0.0m)
                return 0.0;

            double output = ((double)percentChange / (averageHigh)) / scaling;
            if (output > 1.0)
                return 1.0;
            return output;
        }

        private double ToLowOutput(decimal percentChange, double averageHigh, double scaling)
        {
            if (percentChange > 0.0m)
                return 0.0;

            double output = ((double)percentChange / (averageHigh)) / scaling;
            if (output > 1.0)
                return 1.0;
            return output;
        }

    }
}
