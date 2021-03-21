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

            var trainingData = dataset.Take(trainingCount);//.Where(x => x.Output.Signal > 0.9m || x.Output.Signal < 0.1m);
            var testData = dataset.Skip(trainingCount);

            

            var network = new DeepBeliefNetwork(new BernoulliFunction(), 
                firstFeature.Input.Data.Length, 
                firstFeature.Input.Data.Length * 2,
                firstFeature.Input.Data.Length * 2,
                //firstFeature.Input.Data.Length * 2,
                //firstFeature.Input.Data.Length / 4,
                //firstFeature.Input.Data.Length,
                //firstFeature.Input.Data.Length,
                3);
            new GaussianWeights(network).Randomize();
            network.UpdateVisibleWeights();

            var teacher = new BackPropagationLearning(network)
            {
                LearningRate = 0.1,
                Momentum = 0.5
                
            };

            int updateEveryEpoch = 1;
            int epochDelay = 1;
            double uncertaintyRate = 0.33;
            double factor = 0.99;
            int epochs = 5000;
            int batchSize = 1000;
            int numBatches = trainingData.Count() / batchSize;
            int i = 0;

            double[][] trainingInputs;
            double[][] trainingOutputs;
            GetTrainingData(trainingData, out trainingInputs, out trainingOutputs, true);
            for (i = 0; i < epochs; i++)
            {
                double epochError = 0.0;
                for(int j = 0; j < numBatches; ++j)
                {
                    var batchInput = trainingInputs.Skip(j * batchSize).Take(batchSize).ToArray();
                    var batchOutput = trainingOutputs.Skip(j * batchSize).Take(batchSize).ToArray();
                    if (i >= updateEveryEpoch && i >= epochDelay && i % updateEveryEpoch == 0)
                    {
                        UpdateConfidences(network, batchInput, batchOutput, factor, uncertaintyRate);
                    }
                    double error = teacher.RunEpoch(batchInput, batchOutput);
                    Console.WriteLine($"({i},{j}): {error}");
                    epochError += error;
                }
                Console.WriteLine($"Epoch {i} error: {epochError}");
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Training set accuracy: {RunTest(trainingData, network, trainingOutputMapper, 0.5, 0.5)}");
                    Console.WriteLine($"Validation set accuracy: {RunTest(testData, network, trainingOutputMapper, 0.5, 0.5)}");
                }
            }

            Console.WriteLine($"Validation set accuracy: {RunTest(testData, network, trainingOutputMapper, 0.5, 0.5)}");

            return Result.Success<ModelMetrics>(null);
        }

        private void UpdateConfidences(DeepBeliefNetwork network, double[][] input, double[][] output, double factor, double p)
        {
            var confidences = new List<(int index, double confidence)>();
            for(int i = 0; i < input.Length; ++i)
            {
                var prediction = network.Compute(input[i]);
                var c = Math.Abs(prediction[0] - prediction[1]);
                //favor ones that are wrong!
                if ((prediction[0] > 0.5 && output[i][0] < 0.5) ||
                    (prediction[1] > 0.5 && output[i][1] < 0.5))
                {
                    c = 0.0;
                }
                confidences.Add((i, c));
            }

            double biasAverage = output.Average(x => x[2]);

            confidences = confidences.OrderBy(x => x.confidence).ToList();
            for(int i = 0; i < confidences.Count * p; ++i)
            {
                output[confidences[i].index][0] *= factor; 
                output[confidences[i].index][1] *= factor;
                output[confidences[i].index][2] *= factor;
            }
        }

        private void GetTrainingData(IEnumerable<(FeatureVector Input, StockData Output)> dataset,
            out double[][] inputs, out double[][] output, bool shuffle = true)
        {
            var trainingData = dataset.ToList();
            if(shuffle)
            {
                Random rnd = new Random();
                trainingData = trainingData.OrderBy(x => rnd.Next()).ToList();
            }

            inputs = trainingData.Select(x => x.Input.Data).ToArray();
            output = trainingData.Select(x =>
                new double[] {
                    x.Output.PercentChangeFromPreviousClose > 0.0m ? 1.0 : 0.0,
                    x.Output.PercentChangeFromPreviousClose > 0.0m ? 0.0 : 1.0,
                    1.0
                }).ToArray();
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
            double biasSum = 0.0;

            var predictionResults = new List<(double PredictedHighSignal, double PredictedLowSignal, double Confidence, double ActualSignal)>();
            foreach (var testSample in testData)
            {
                var prediction = network.Compute(testSample.Input.Data);

                predictionResults.Add((prediction[0], prediction[1], Math.Abs(prediction[0] - prediction[1]),
                        testSample.Output.PercentChangeFromPreviousClose > 0.0m ? 1.0 : 0.0));
                //predictionResults.Add((prediction[0], prediction[1], Math.Abs(prediction[0] - prediction[1]),
                //        testSample.Output.PercentChangeHighToPreviousClose + testSample.Output.PercentChangeLowToPreviousClose > 0.0m ? 1.0 : 0.0));

                biasSum += prediction[2];
            }

            predictionResults = predictionResults.OrderByDescending(x => x.Confidence).ToList();
            var runningAccuracy = new List<double>();
            double correct = 0.0;
            int i = 0;
            foreach (var result in predictionResults)
            {
                if ((result.PredictedHighSignal > highThreshold && result.ActualSignal > highThreshold) || 
                    (result.PredictedHighSignal < lowThreshold && result.ActualSignal < lowThreshold))
                    correct++;

                runningAccuracy.Add(correct / (double)(i + 1));
                i++;
            }

            var accuracyByPercentile = new double[] { 0.01, 0.05, 0.10, 0.25, 0.50, 1.0 };
            foreach(var percent in accuracyByPercentile)
            {
                int index = (int)(runningAccuracy.Count() * percent);
                if(index >= runningAccuracy.Count)
                {
                    index = runningAccuracy.Count - 1;
                }
                Console.WriteLine($"\t{percent * 100.0}%: {runningAccuracy[index]}, Conf: {predictionResults[index].Confidence}");
            }

            Console.WriteLine($"\tBias Average: {biasSum / runningAccuracy.Count}");
            return runningAccuracy.LastOrDefault();
        }
    }
}
