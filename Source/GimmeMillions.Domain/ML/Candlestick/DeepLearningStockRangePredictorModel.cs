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
        private PolynomialRegression _sentimentScaler;
        private PolynomialRegression _predictionScaler;
        private int _maxEpochs = 100;
        private int _resetEpoch = 40;
        private double _outputScaling = 1.0;
        private double _averageGain = 0.0;
        private double _medianGain = 0.0;
    
        public DeepLearningStockRangePredictorModel(int maxEpochs = 100,
            int resetEpoch = 40,
            double outputScaling = 1.0)
        {
            _maxEpochs = maxEpochs;
            _resetEpoch = resetEpoch;
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
            _resetEpoch = (int)config["resetEpoch"];
            _outputScaling = (double)config["outputScaling"];
            _averageGain = (double)config["averageGain"];
            _medianGain = (double)config["medianGain"];

            _predictionScaler = new PolynomialRegression((int)config["psDegree"]);
            _predictionScaler.Weights = config["psWeights"].ToObject<double[]>();
            _predictionScaler.Intercept = (double)config["psIntercept"];

            _sentimentScaler = new PolynomialRegression((int)config["ssDegree"]);
            _sentimentScaler.Weights = config["ssWeights"].ToObject<double[]>();
            _sentimentScaler.Intercept = (double)config["ssIntercept"];

            return Result.Success();
        }

        public StockRangePrediction Predict(FeatureVector Input)
        {
            if(_network == null)
            {
                return new StockRangePrediction();
            }

            var prediction = _network.Compute(Input.Data);
            double p = prediction[0];
            if(_predictionScaler != null)
            {
                //p = _predictionScaler.Transform(prediction[0]);
            }
            double s = Math.Abs(prediction[0] - prediction[1]);
            //double s = prediction[2];
            //if (_sentimentScaler != null)
            //{
            //    s = _sentimentScaler.Transform(prediction[2]);
            //}

            var predictedGain = (p - 0.50);
            double sentiment = s * 100.0;
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
                resetEpoch = _resetEpoch,
                outputScaling = _outputScaling,
                averageGain = _averageGain,
                medianGain = _medianGain,
                psWeights = _predictionScaler.Weights,
                psIntercept = _predictionScaler.Intercept,
                psDegree = _predictionScaler.Degree,
                ssWeights = _sentimentScaler.Weights,
                ssIntercept = _sentimentScaler.Intercept,
                ssDegree = _sentimentScaler.Degree,
            });

            _network.Save(pathToModel);
            //_predictionScaler.(pathToModel + ".ps");
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

            dataset = dataset.OrderBy(x => x.Output.Date).ToList();
            var firstFeature = dataset.FirstOrDefault();
            int trainingCount = (int)((double)dataset.Count() * (1.0 - testFraction));

            var trainingData = dataset.Take(trainingCount).ToList();//.Where(x => x.Output.Signal > 0.9m || x.Output.Signal < 0.1m);
            var testData = dataset.Skip(trainingCount).ToList();


            _medianGain = (double)trainingData.OrderBy(x => x.Output.PercentChangeFromPreviousClose)
                .ToList()[trainingData.Count / 2].Output.PercentChangeFromPreviousClose;

            _averageGain = (double)trainingData.Average(x => Math.Abs((double)x.Output.PercentChangeFromPreviousClose - _medianGain));

            _network = new DeepBeliefNetwork(new BernoulliFunction(), 
                firstFeature.Input.Data.Length, 
                firstFeature.Input.Data.Length,
                //firstFeature.Input.Data.Length * 2,
                //firstFeature.Input.Data.Length * 2,
                firstFeature.Input.Data.Length,
                4);

            var rngWeights = new GaussianWeights(_network); 
            rngWeights.Randomize();
            _network.UpdateVisibleWeights();

            var teacher = new BackPropagationLearning(_network)
            {
                LearningRate = 0.01,
                Momentum = 0.1
                
            };

            double[][] trainingInputs;
            double[][] trainingOutputs;
            double bestModel = 0.0;
            
            GetTrainingData(trainingData, out trainingInputs, out trainingOutputs, true);
            for (int i = 0; i < _maxEpochs; i++)
            {
                //if (i % _resetEpoch == 0)
                //{
                //    rngWeights.Randomize();
                //    //GetTrainingData(trainingData, out trainingInputs, out trainingOutputs, true);
                //}

                UpdateConfidences(_network, trainingInputs, trainingOutputs, 0.99);
                double epochError = 0.0;
                
                double error = teacher.RunEpoch(trainingInputs, trainingOutputs);
                Console.Write(".");
                epochError += error;

                Console.WriteLine($"Epoch {i} error: {epochError}");

                if (i % 10 == 0)
                {
                    double topAccuracy = Evaluate(trainingData, _network, trainingOutputMapper, 0.5, 0.5, true);
                    Console.WriteLine($"Training set accuracy: {topAccuracy}"); 
                    
                }

                if (testData.Any())
                {
                    var validationAccuracy = Evaluate(testData, _network, trainingOutputMapper, 0.5, 0.5, i % 10 == 0);
                    Console.WriteLine($"Validation set accuracy: {validationAccuracy}");
                    if (validationAccuracy >= bestModel)
                    {
                        bestModel = validationAccuracy;
                        Console.WriteLine($"Caching model...");
                        _network.Save("best_model_cache");
                    }

                }
                else
                {
                    _network.Save("best_model_cache");
                }
            }

            //Console.WriteLine($"Loading best model ({bestModel})...");
            _network = DeepBeliefNetwork.Load("best_model_cache");
            Evaluate(trainingData, _network, trainingOutputMapper, 0.5, 0.5, true);
            if (testData.Any())
            {
                Evaluate(testData, _network, trainingOutputMapper, 0.5, 0.5, true);
            }

            GetTrainingData(trainingData, out trainingInputs, out trainingOutputs, true);
            TrainOutputScaler(_network, trainingInputs);

            return Result.Success<ModelMetrics>(null);
        }

        private void UpdateConfidences(DeepBeliefNetwork network, double[][] input, double[][] output, double factor)
        {
            var confidences = new List<(int index, double confidence)>();
            for(int i = 0; i < input.Length; ++i)
            {
                var prediction = network.Compute(input[i]);

                double confidence = Math.Abs(prediction[0] - prediction[1]);
                ////Prioritize wrong predictions
                if ((prediction[0] > prediction[1] && output[i][0] < 0.5) ||
                    (prediction[0] < prediction[1] && output[i][0] > 0.5))
                {
                    confidence *= -1.0;
                }
                confidences.Add((i, confidence));
                
            }

            confidences = confidences.OrderByDescending(x => x.confidence).ToList();
            double c = 0.0;
            for (int i = 0; i < confidences.Count; ++i)
            {
                c = (1.0 - ((double)i / (double)confidences.Count));
                output[confidences[i].index][2] =
                    (output[confidences[i].index][2] * factor) +
                    c * (1.0 - factor);
                output[confidences[i].index][3] = 1.0 - output[confidences[i].index][2];
                //output[confidences[i].index][2] = Math.Abs(output[confidences[i].index][0] - output[confidences[i].index][1]);

                //output[confidences[i].index][2] = c;

            }

        }

        private void GetTrainingData(IEnumerable<(FeatureVector Input, StockData Output)> dataset,
            out double[][] inputs, out double[][] output, bool shuffle = true)
        {
            var trainingData = dataset.ToList();

            //var posSamples = trainingData.Where(x => x.Output.PercentChangeFromPreviousClose > 0.0m);
           // var negSamples = trainingData.Where(x => x.Output.PercentChangeFromPreviousClose < 0.0m);

            if (shuffle)
            {
                Random rnd = new Random();
                trainingData = trainingData.OrderBy(x => rnd.Next()).ToList();
                //posSamples = posSamples.OrderBy(x => rnd.Next()).ToList();
                //negSamples = negSamples.OrderBy(x => rnd.Next()).ToList();
            }

            //int pnSampleCount = Math.Min((int)posSamples.Count(), (int)negSamples.Count());

            //var samples = posSamples.Take(pnSampleCount).ToList();
            //samples.AddRange(negSamples.Take(pnSampleCount));

            var activationFunction = new BernoulliFunction();

            inputs = trainingData.Select(x => x.Input.Data).ToArray();
            output = trainingData.Select(x =>
            {
                double target = activationFunction.Function(((double)x.Output.PercentChangeFromPreviousClose - _medianGain) / _averageGain);
                //double target = (double)x.Output.PercentChangeFromPreviousClose > 0.0 ? 1.0 : 0.0;
                return new double[] {
                    target,
                    1.0 - target,
                    0.5,
                    0.5
                };
            }).ToArray();
                
        }

        private double Evaluate(IEnumerable<(FeatureVector Input, StockData Output)> testData, 
            DeepBeliefNetwork network, 
            ITrainingOutputMapper trainingOutputMapper,
            double lowThreshold, double highThreshold, bool verbose = false)
        {
            var predictionResults = new List<(double PredictedHighSignal, double PredictedLowSignal, double Confidence, double ActualSignal)>();
            double error = 0.0;
            var activationFunction = new BernoulliFunction();
            foreach (var testSample in testData)
            {
                var prediction = network.Compute(testSample.Input.Data);
                //double target = activationFunction.Function(testSample.Output.PercentChangeFromPreviousClose > _medianGain ?
                //   (double)testSample.Output.PercentChangeFromPreviousClose / (_averageGain) :
                //   (double)testSample.Output.PercentChangeFromPreviousClose / (_averageLoss));
                //error += Math.Abs(target - prediction[0]) + Math.Abs(1.0 - target - prediction[1]);

                //predictionResults.Add((prediction[0], prediction[1], Math.Abs(prediction[0] - prediction[1]),
                //        testSample.Output.Signal > 0.5m ? 1.0 : 0.0));
                double confidence = Math.Abs(prediction[0] - prediction[1]);
                //confidence = prediction[2];

                predictionResults.Add((prediction[0], prediction[1], confidence,
                        (double)testSample.Output.PercentChangeFromPreviousClose - _medianGain > 0.0 ? 1.0 : 0.0));

                //biasSum += prediction[2];
            }

            predictionResults = predictionResults.OrderByDescending(x => x.Confidence).ToList();
            var runningAccuracy = new List<double>();
            double correct = 0.0;
            int TP = 0, TN = 0;
            int i = 0;
            foreach (var result in predictionResults)
            {
                if (result.PredictedHighSignal > highThreshold && result.ActualSignal > highThreshold)
                {
                    correct++;
                    TP++;
                }
                if (result.PredictedHighSignal < lowThreshold && result.ActualSignal < lowThreshold)
                {
                    correct++;
                    TN++;
                }

                runningAccuracy.Add(correct / (double)(i + 1));
                i++;
            }

            if(verbose)
            { 
                var accuracyByPercentile = new double[] { 0.01, 0.05, 0.10, 0.25, 0.50, 1.0 };
                for (i = 0; i < accuracyByPercentile.Length; ++i)
                {
                    int index = (int)(runningAccuracy.Count() * accuracyByPercentile[i]);
                    if (index >= runningAccuracy.Count)
                    {
                        index = runningAccuracy.Count - 1;
                    }

                    Console.WriteLine($"\t{accuracyByPercentile[i] * 100.0}%: {runningAccuracy[index]}, Conf: {predictionResults[index].Confidence}");
                }

                Console.WriteLine($"\tTP: {TP}, TN: {TN}");
            }
           
            //return the top 50% accuracy
            return runningAccuracy[runningAccuracy.Count / 10];
        }

        private void TrainOutputScaler(DeepBeliefNetwork network, double[][] input)
        {
            var sentiments = new List<double>();
            var predictions = new List<double>();
            var outputs = new List<double>();
            foreach(var sample in input)
            {
                var p = network.Compute(sample);
                //sentiments.Add(p[2]);
                sentiments.Add(Math.Abs(p[1] - p[0]));
                predictions.Add(p[0]);
            }

            sentiments = sentiments.OrderBy(x => x).ToList();
            predictions = predictions.OrderBy(x => x).ToList();
            for(int i = 0; i < sentiments.Count; ++i)
            {
                outputs.Add((double)i / (double)sentiments.Count);
            }

            var ls = new PolynomialLeastSquares()
            {
                Degree = 2
            };

            _sentimentScaler = ls.Learn(sentiments.ToArray(), outputs.ToArray());
            _predictionScaler = ls.Learn(predictions.ToArray(), outputs.ToArray());

        }
    }
}
