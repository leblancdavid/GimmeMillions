﻿using Accord.MachineLearning.VectorMachines.Learning;
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GimmeMillions.Domain.ML.Candlestick
{
    public class DeepLearningStockRangePredictorModel : IStockRangePredictor
    {
        private DeepBeliefNetwork _network;
        private int _maxEpochs = 100;
        private int _batchSize = 1000;
        private double _outputScaling = 1.5;
        private double _averageGain = 0.0;
        private double _averageLoss = 0.0;
    
        public DeepLearningStockRangePredictorModel(int maxEpochs = 100,
            int batchSize = 1000,
            double outputScaling = 1.5)
        {
            _maxEpochs = maxEpochs;
            _batchSize = batchSize;
            _outputScaling = outputScaling;
        }
        public bool IsTrained => _network != null;

        public Result Load(string pathToModel)
        {
            _network = DeepBeliefNetwork.Load(pathToModel);
            if (_network == null)
                return Result.Failure($"Unable to load model '{pathToModel}'");

            var config = JObject.Parse(File.ReadAllText(pathToModel + ".config.json"));
            _maxEpochs = (int)config["maxEpochs"];
            _batchSize = (int)config["batchSize"];
            _outputScaling = (double)config["outputScaling"];
            _averageGain = (double)config["averageGain"];
            _averageLoss = (double)config["averageLoss"];

            return Result.Success();
        }

        public StockRangePrediction Predict(FeatureVector Input)
        {
            if(_network == null)
            {
                return new StockRangePrediction();
            }

            var prediction = _network.Compute(Input.Data);

            var predictedGain = prediction[0] > 0.5 ?
                (prediction[0] - 0.50) * 2.0 * _averageGain * _outputScaling :
                (prediction[0] - 0.50) * 2.0 * _averageLoss * _outputScaling;
            double sentiment = Math.Abs(prediction[0] - prediction[1]) * 100.0;
            if (predictedGain < 0.0)
                sentiment *= -1.0;

            return new StockRangePrediction()
            {
                PredictedHigh = predictedGain,
                PredictedLow = predictedGain,
                Sentiment = sentiment,
                Confidence = Math.Abs(sentiment)
            };
        }

        public Result Save(string pathToModel)
        {
            if (_network == null)
                return Result.Failure($"Model has not been trained yet");

            var config = JObject.FromObject(new
            {
                maxEpochs = _maxEpochs,
                batchSize = _batchSize,
                outputScaling = _outputScaling,
                averageGain = _averageGain,
                averageLoss = _averageLoss
            });

            _network.Save(pathToModel);
            File.WriteAllText(pathToModel + ".config.json", config.ToString());
            return Result.Success();
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

            _network = new DeepBeliefNetwork(new BernoulliFunction(), 
                firstFeature.Input.Data.Length, 
                firstFeature.Input.Data.Length,
                firstFeature.Input.Data.Length,
                firstFeature.Input.Data.Length,
                //(int)(firstFeature.Input.Data.Length * 0.5),
                //(int)(firstFeature.Input.Data.Length * 0.2),
                3);
            new GaussianWeights(_network).Randomize();
            _network.UpdateVisibleWeights();

            var teacher = new BackPropagationLearning(_network)
            {
                LearningRate = 0.5,
                Momentum = 0.5
                
            };

            int numBatches = trainingData.Count() / _batchSize;
            double[][] trainingInputs;
            double[][] trainingOutputs;
            GetTrainingData(trainingData, out trainingInputs, out trainingOutputs, true);
            for (int i = 1; i <= _maxEpochs; i++)
            {
                double epochError = 0.0;
                for(int j = 0; j < numBatches; ++j)
                {
                    var batchInput = trainingInputs.Skip(j * _batchSize).Take(_batchSize).ToArray();
                    var batchOutput = trainingOutputs.Skip(j * _batchSize).Take(_batchSize).ToArray();
                    double error = teacher.RunEpoch(batchInput, batchOutput);
                    Console.Write(".");
                    epochError += error;
                }

                Console.WriteLine($"Epoch {i} error: {epochError}");

                if (i % 10 == 0)
                {
                    double topAccuracy = Evaluate(trainingData, _network, trainingOutputMapper, 0.5, 0.5);
                    Console.WriteLine($"Training set accuracy: {topAccuracy}"); 
                    if (testData.Any())
                        Console.WriteLine($"Validation set accuracy: {Evaluate(testData, _network, trainingOutputMapper, 0.5, 0.5)}");

                }


                UpdateConfidences(_network, trainingInputs, trainingOutputs, 0.99, 0.5);
            }

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
                //output[confidences[i].index][0] *= factor; 
                //output[confidences[i].index][1] *= factor;
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

            var activationFunction = new BernoulliFunction();
            _averageGain = (double)trainingData.Where(x => x.Output.PercentChangeFromPreviousClose > 0.0m)
                .Average(x => Math.Abs(x.Output.PercentChangeFromPreviousClose));
            _averageLoss = (double)trainingData.Where(x => x.Output.PercentChangeFromPreviousClose <= 0.0m)
                .Average(x => Math.Abs(x.Output.PercentChangeFromPreviousClose));
            inputs = trainingData.Select(x => x.Input.Data).ToArray();
            output = trainingData.Select(x =>
            {
                double target = activationFunction.Function(x.Output.PercentChangeFromPreviousClose > 0.0m ?
                    (double)x.Output.PercentChangeFromPreviousClose / (_averageGain) :
                    (double)x.Output.PercentChangeFromPreviousClose / (_averageLoss));
                return new double[] {
                    target,
                    1.0 - target,
                    1.0
                };
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

        private double Evaluate(IEnumerable<(FeatureVector Input, StockData Output)> testData, 
            DeepBeliefNetwork network, 
            ITrainingOutputMapper trainingOutputMapper,
            double lowThreshold, double highThreshold)
        {
            var predictionResults = new List<(double PredictedHighSignal, double PredictedLowSignal, double Confidence, double ActualSignal)>();

            var activationFunction = new BernoulliFunction();
            double totalError = 0.0;
            foreach (var testSample in testData)
            {
                var prediction = network.Compute(testSample.Input.Data);
                double target = activationFunction.Function(testSample.Output.PercentChangeFromPreviousClose > 0.0m ?
                    (double)testSample.Output.PercentChangeFromPreviousClose / (_averageGain) :
                    (double)testSample.Output.PercentChangeFromPreviousClose / (_averageLoss));
                totalError += Math.Abs(target - prediction[0]);
                //predictionResults.Add((prediction[0], prediction[1], Math.Abs(prediction[0] - prediction[1]),
                //        testSample.Output.Signal > 0.5m ? 1.0 : 0.0));
                double confidence = Math.Abs(prediction[0] - prediction[1]);
                confidence = prediction[2];
                predictionResults.Add((prediction[0], prediction[1], confidence,
                        testSample.Output.PercentChangeFromPreviousClose > 0.0m ? 1.0 : 0.0));

                //biasSum += prediction[2];
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
            double topAccuracy = 0.0;
            for(i = 0; i < accuracyByPercentile.Length; ++i)
            {
                int index = (int)(runningAccuracy.Count() * accuracyByPercentile[i]);
                if(index >= runningAccuracy.Count)
                {
                    index = runningAccuracy.Count - 1;
                }

                if(i == 0)
                {
                    topAccuracy = runningAccuracy[index];
                }

                Console.WriteLine($"\t{accuracyByPercentile[i] * 100.0}%: {runningAccuracy[index]}, Conf: {predictionResults[index].Confidence}");
            }

            Console.WriteLine($"Error: {totalError}");

            return topAccuracy;
        }
    }
}
