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
using GimmeMillions.Domain.ML.ActivationFunctions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.ML.Candlestick
{
    public class DeepLearningStockRangePredictorModel : IStockRangePredictor
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
                //ToHighOutput(x.Output.PercentChangeHighToPreviousClose, averageHigh, 3.0),
                //ToLowOutput(x.Output.PercentChangeLowToPreviousClose, averageLow, 3.0),
                }).ToArray();

            var network = new DeepBeliefNetwork(new BernoulliFunction(0.5), 
                firstFeature.Input.Data.Length, 
                firstFeature.Input.Data.Length,
                firstFeature.Input.Data.Length,
                firstFeature.Input.Data.Length,
                1);
            new GaussianWeights(network).Randomize();
            network.UpdateVisibleWeights();

            var teacher = new BackPropagationLearning(network)
            {
                LearningRate = 0.1,
                Momentum = 0.5
                
            };
            //var teacher = new DeepNeuralNetworkLearning(network)
            //{
            //    Algorithm = (ann, i) => new ParallelResilientBackpropagationLearning(ann),
            //    LayerIndex = network.Layers.Length - 1,
            //};

            //double[][] layerData = teacher.GetLayerInput(trainingInputs);

            int epochs = 5000;
            int i = 0;
            for (i = 0; i < epochs; i++)
            {
                double error = teacher.RunEpoch(trainingInputs, signalOutputs);
                Console.WriteLine($"({i}): {error}");
                if(i % 10 == 0)
                {
                    Console.WriteLine($"Training set accuracy: {RunTest(trainingData, network, trainingOutputMapper, 0.4, 0.6)}");
                    Console.WriteLine($"Validation set accuracy: {RunTest(testData, network, trainingOutputMapper, 0.4, 0.6)}");
                    //Console.WriteLine($"Training set accuracy: {RunTest(trainingData, network, trainingOutputMapper, 0.5, 0.5)}, " +
                    //    $"{RunTestWithCheck(trainingData, network, averageHigh, averageLow, 3.0)}");
                    //Console.WriteLine($"Validation set accuracy: {RunTest(testData, network, trainingOutputMapper, 0.5, 0.5)}, " +
                    //    $"{RunTestWithCheck(testData, network, averageHigh, averageLow, 3.0)}");
                }
            }

            Console.WriteLine($"Validation set accuracy: {RunTest(testData, network, trainingOutputMapper, 0.4, 0.6)}");
            //Console.WriteLine($"Validation set accuracy: {RunTest(testData, network, trainingOutputMapper, 0.5, 0.5)}, " +
            //    $"{RunTestWithCheck(testData, network, averageHigh, averageLow, 3.0)}");

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

        private double RunTest(IEnumerable<(FeatureVector Input, StockData Output)> testData, 
            DeepBeliefNetwork network, 
            ITrainingOutputMapper trainingOutputMapper,
            double lowThreshold, double highThreshold)
        {
            var predictionResults = new List<(double PredictedSignal, double ActualSignal)>();
            foreach (var testSample in testData)
            {
                var prediction = network.Compute(testSample.Input.Data);
                //predictionResults.Add((prediction[0], trainingOutputMapper.GetOutputValue(testSample.Output)));

                //var prediction = signalModel.Score(testSample.Input.Data);
                if (prediction[0] > highThreshold)
                    predictionResults.Add((prediction[0], trainingOutputMapper.GetOutputValue(testSample.Output)));

                if (prediction[0] < lowThreshold)
                    predictionResults.Add((prediction[0], trainingOutputMapper.GetOutputValue(testSample.Output)));
            }

            predictionResults = predictionResults.OrderByDescending(x => x.PredictedSignal).ToList();
            var runningAccuracy = new List<double>();
            double correct = 0.0;
            int i = 0;
            foreach (var result in predictionResults)
            {
                if ((result.PredictedSignal > highThreshold && result.ActualSignal > highThreshold) || 
                    (result.PredictedSignal < lowThreshold && result.ActualSignal < lowThreshold))
                    correct++;

                runningAccuracy.Add(correct / (double)(i + 1));
                i++;
            }

            return runningAccuracy.LastOrDefault();
        }

        private double RunTestWithCheck(IEnumerable<(FeatureVector Input, StockData Output)> testData,
           DeepBeliefNetwork network,
           double averageHigh, double averageLow, double scaling)
        {
            double correct = 0.0;
            foreach (var testSample in testData)
            {
                var prediction = network.Compute(testSample.Input.Data);
                if((prediction[1] > prediction[2] && 
                    ToHighOutput(testSample.Output.PercentChangeHighToPreviousClose, averageHigh, scaling) >
                    ToLowOutput(testSample.Output.PercentChangeLowToPreviousClose, averageLow, scaling)) ||
                    (prediction[2] > prediction[1] &&
                    ToHighOutput(testSample.Output.PercentChangeHighToPreviousClose, averageHigh, scaling) <
                    ToLowOutput(testSample.Output.PercentChangeLowToPreviousClose, averageLow, scaling)))
                {
                    correct++;
                }

               
            }

            correct /= testData.Count();
            return correct;
        }

    }
}
