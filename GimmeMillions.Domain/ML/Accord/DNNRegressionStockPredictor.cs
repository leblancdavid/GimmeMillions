using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using Accord.Statistics.Kernels;
using Accord.Statistics.Models.Regression.Linear;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Logging;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Accord
{
    public class DNNRegressionStockPredictor : IStockPredictionModel<FeatureVector>
    {
        public string StockSymbol { get; set; }

        public string Encoding { get; private set; }

        public bool IsTrained { get; private set; }
        private IEnumerable<ILogger> _loggers;

        public DNNRegressionStockPredictor(IEnumerable<ILogger> loggers = null)
        {
            _loggers = loggers;
        }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            throw new NotImplementedException();
        }

        public StockPrediction Predict(FeatureVector Input)
        {
            throw new NotImplementedException();
        }

        public Result Save(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            //var averageValue = dataset.Average(x => x.Output.PercentChangeHighToPreviousClose);
            var averageValue = 0.0m;
            int trainingSize = (int)(dataset.Count() * (1.0 - testFraction));
            var trainingInputData = dataset.Take(trainingSize).Select(x => x.Input.Data).ToArray();
            var trainingOuputData = dataset.Take(trainingSize).Select(x => new double[]
            {
                //(double)(x.Output.PercentChangeFromPreviousClose / 10.0m)
                x.Output.PercentDayChange > averageValue ? 1.0 : 0.0,
                x.Output.PercentDayChange <= averageValue ? 1.0 : 0.0,
                0.0 //outlier
                //(double)(x.Output.PercentChangeHighToPreviousClose / 10.0m)
                //x.Output.PercentChangeFromPreviousClose > 0.0m ? (double)(x.Output.PercentChangeFromPreviousClose / 10.0m) : 0.0,
                //x.Output.PercentChangeFromPreviousClose <= 0.0m ? (double)(x.Output.PercentChangeFromPreviousClose / 10.0m) : 0.0,
            }).ToArray();
            var testInputData = dataset.Skip(trainingSize).Select(x => x.Input.Data).ToArray();
            var testOuputData = dataset.Skip(trainingSize).Select(x => new double[]
            {
                //(double)(x.Output.PercentChangeFromPreviousClose / 10.0m),
                x.Output.PercentDayChange > averageValue ? 1.0 : 0.0,
                x.Output.PercentDayChange <= averageValue ? 1.0 : 0.0,
                0.0 //outlier
                //(double)(x.Output.PercentChangeHighToPreviousClose / 10.0m)
                //x.Output.PercentChangeFromPreviousClose > 0.0m ? (double)(x.Output.PercentChangeFromPreviousClose / 10.0m) : 0.0,
                //x.Output.PercentChangeFromPreviousClose <= 0.0m ? (double)(x.Output.PercentChangeFromPreviousClose / 10.0m) : 0.0,
            }).ToArray();

            int featureLength = trainingInputData[0].Length;
            var hiddenLayers = new int[]
            {
                featureLength * 4,
                featureLength * 2,
                featureLength,
                featureLength / 2,
                featureLength / 4,
                3
            };

            if (_loggers != null)
            {
                foreach (var logger in _loggers)
                {
                    logger.Log($"Training DNN with hidden layers: {String.Join(",", hiddenLayers.Select(p => p.ToString()).ToArray())}");
                }
            }

            var dnn = new ActivationNetwork(new BipolarSigmoidFunction(), 
                featureLength,
                hiddenLayers);
            new NguyenWidrow(dnn).Randomize();
            var dnnLearner = new ParallelResilientBackpropagationLearning(dnn);

            double runningError = 0.0;
            int maxEpochs = 20000;
            int accuracyCheck = 50;
            int dataFilter = 200;
            double errorThreshold = 1.0;
            double filterFactor = 0.75;
            double outlierIncrement = 0.1;

            var outliers = new List<(double[] Input, double[] Output)>();
            for(int epoch = 1; epoch <= maxEpochs; epoch++)
            {
                runningError = dnnLearner.RunEpoch(trainingInputData, trainingOuputData);
                if(runningError < errorThreshold)
                {
                    break;
                }
                if (_loggers != null)
                {
                    if (epoch % accuracyCheck == 0)
                    {
                        var trainMetrics = Evaluate(dnn, trainingInputData, trainingOuputData);
                        var testMetrics = Evaluate(dnn, testInputData, testOuputData);
                        foreach (var logger in _loggers)
                        {
                            logger.Log($"Epoch({epoch}) - {runningError.ToString()}");
                            logger.Log($"Training results: {trainMetrics.ToString()}");
                            logger.Log($"Test results: {testMetrics.ToString()}");
                        }
                    }
                }

                //if ((epoch) % dataFilter == 0)
                //{
                //    if (_loggers != null)
                //    {
                //        foreach (var logger in _loggers)
                //        {
                //            logger.Log($"Epoch({epoch + 1}) - Fixing outliers...");
                //        }
                //    }

                //    FixOutliers(dnn, trainingInputData, trainingOuputData, filterFactor, outlierIncrement);
                //    //var filteredData = FilterOutliers(dnn, trainingInputData, trainingOuputData, filterFactor, ref outliers);
                //    //trainingInputData = filteredData.Input;
                //    //trainingOuputData = filteredData.Output;
                //}

            }

            //var outlierInputs = outliers.Select(x => x.Input).ToArray();
            //var outlierOutputs = outliers.Select(x => x.Output).ToArray();
            //var outlierDnn = new ActivationNetwork(new SigmoidFunction(),
            //   featureLength,
            //   hiddenLayers);
            //new NguyenWidrow(outlierDnn).Randomize();
            //dnnLearner = new ParallelResilientBackpropagationLearning(outlierDnn);
            //if (_loggers != null)
            //{
            //    foreach (var logger in _loggers)
            //    {
            //        logger.Log($"Training outliers DNN...");
            //    }
            //}
            //for (int epoch = 1; epoch <= maxEpochs; epoch++)
            //{
            //    runningError = dnnLearner.RunEpoch(outlierInputs, outlierOutputs);
            //    if (runningError < errorThreshold)
            //    {
            //        break;
            //    }
            //    if (_loggers != null)
            //    {
            //        if (epoch % accuracyCheck == 0)
            //        {
            //            var trainMetrics = Evaluate(dnn, outlierDnn, outlierInputs, outlierOutputs);
            //            var testMetrics = Evaluate(dnn, outlierDnn, testInputData, testOuputData);
            //            foreach (var logger in _loggers)
            //            {
            //                logger.Log($"Epoch({epoch}) - {runningError.ToString()}");
            //                logger.Log($"Training results: {trainMetrics.ToString()}");
            //                logger.Log($"Test results: {testMetrics.ToString()}");
            //            }
            //        }
            //    }
            //}

            return Result.Ok(Evaluate(dnn, testInputData, testOuputData));

        }

        private ModelMetrics Evaluate(ActivationNetwork dnn, double[][] input, double[][] output)
        {
            double tp = 1.0, tn = 1.0, fp = 1.0, fn = 1.0;
            double avgError = 0.0;
            var errorByInput = new List<(double Output, double Error, bool Correct)>();
            for (int i = 0; i < input.Length; ++i)
            {
                var result = dnn.Compute(input[i]);
                //if (result[2] > 0.5)
                //{
                //    //if we think it might be an outlier, then just ignore
                //    continue;
                //}
                var computedOutput = result[0] > Math.Abs(result[1]) ? result[0] : -1.0 * result[1];
                var expectedOutput = output[i][0] > Math.Abs(output[i][1]) ? output[i][0] : -1.0 * output[i][1];
                //var computedOutput = result[0];
                //var expectedOutput = output[i][0];
                bool correct = false;
                if (expectedOutput > 0)
                {
                    if(computedOutput > 0)
                    {
                        tp++;
                        correct = true;
                    }
                    else
                    {
                        fn++;
                    }
                }
                else
                {
                    if (computedOutput <= 0)
                    {
                        tn++;
                        correct = true;
                    }
                    else
                    {
                        fp++;
                    }
                }

                var error = Math.Abs(computedOutput - expectedOutput);
                errorByInput.Add((computedOutput, error, correct));
                avgError += error;
            }

            var metrics = new ModelMetrics();
            metrics.Accuracy = (tp + tn) / (tp + tn + fp + fn);
            metrics.PositivePrecision = tp / (tp + fp);
            metrics.PositiveRecall = tp / (tp + fn);
            metrics.NegativePrecision = tn / (tn + fn);
            metrics.NegativeRecall = tn / (tn + fp);
            metrics.Error = avgError / output.Length;

            errorByInput = errorByInput.OrderByDescending(x => x.Output).ToList();

            return metrics;
        }

        private ModelMetrics Evaluate(ActivationNetwork dnn, ActivationNetwork outlierDnn, double[][] input, double[][] output)
        {
            double tp = 1.0, tn = 1.0, fp = 1.0, fn = 1.0;
            double avgError = 0.0;

            var errorByInput = new List<(double Error, bool Correct)>();
            for (int i = 0; i < input.Length; ++i)
            {
                var result1 = dnn.Compute(input[i]);
                var computedOutput1 = result1[0] > Math.Abs(result1[1]) ? result1[0] : -1.0 * result1[1];
                var result2 = outlierDnn.Compute(input[i]);
                var computedOutput2 = result2[0] > Math.Abs(result2[1]) ? result2[0] : -1.0 * result2[1];

                var computedOutput = Math.Abs(computedOutput1) > Math.Abs(computedOutput2) ? computedOutput1 : computedOutput2;
                var expectedOutput = output[i][0] > Math.Abs(output[i][1]) ? output[i][0] : -1.0 * output[i][1];
                //var computedOutput = result[0];
                //var expectedOutput = output[i][0];
                bool correct = false;
                if (expectedOutput > 0)
                {
                    if (computedOutput > 0)
                    {
                        tp++;
                        correct = true;
                    }
                    else
                    {
                        fn++;
                    }
                }
                else
                {
                    if (computedOutput <= 0)
                    {
                        tn++;
                        correct = true;
                    }
                    else
                    {
                        fp++;
                    }
                }

                var error = Math.Abs(computedOutput - expectedOutput);
                errorByInput.Add((error, correct));
                avgError += error;
            }

            var metrics = new ModelMetrics();
            metrics.Accuracy = (tp + tn) / (tp + tn + fp + fn);
            metrics.PositivePrecision = tp / (tp + fp);
            metrics.PositiveRecall = tp / (tp + fn);
            metrics.NegativePrecision = tn / (tn + fn);
            metrics.NegativeRecall = tn / (tn + fp);
            metrics.Error = avgError / output.Length;

            errorByInput = errorByInput.OrderBy(x => x.Error).ToList();

            return metrics;
        }

        private (double[][] Input, double[][] Output) FilterOutliers(ActivationNetwork dnn, 
            double[][] input, 
            double[][] output, 
            double filterFactor,
            ref List<(double[] Input, double[] Output)> outliers)
        {
            var errorByIndex = new List<(int Index, double Error)>();
            for (int i = 0; i < input.Length; ++i)
            {
                var result = dnn.Compute(input[i]);
                double error = 0.0;
                for(int j = 0; j < result.Length; ++j)
                {
                    error += Math.Abs(result[j] - output[i][j]);
                }
                errorByIndex.Add((i, error));
            }

            int totalKept = (int)(input.Length * filterFactor);
            errorByIndex = errorByIndex.OrderBy(x => x.Error).ToList();

            var filteredInputs = new double[totalKept][];
            var filteredOutputs = new double[totalKept][];
            for(int i = 0; i < filteredInputs.Length; ++i)
            {
                filteredInputs[i] = input[errorByIndex[i].Index];
                filteredOutputs[i] = output[errorByIndex[i].Index];
            }

            var outlierInputs = new double[input.Length - totalKept][];
            var outlierOutputs = new double[input.Length - totalKept][];
            for (int i = 0; i < input.Length - totalKept; ++i)
            {
                outliers.Add((input[errorByIndex[i + totalKept].Index], output[errorByIndex[i + totalKept].Index]));
            }

            return (filteredInputs, filteredOutputs);
        }

        private void FixOutliers(ActivationNetwork dnn,
           double[][] input,
           double[][] output,
           double filterFactor,
           double outlierIncrement)
        {
            var errorByIndex = new List<(int Index, double Error)>();
            for (int i = 0; i < input.Length; ++i)
            {
                var result = dnn.Compute(input[i]);
                double error = 0.0;
                for (int j = 0; j < result.Length; ++j)
                {
                    error += Math.Abs(result[j] - output[i][j]);
                }
                errorByIndex.Add((i, error));
            }

            int totalKept = (int)(input.Length * filterFactor);
            errorByIndex = errorByIndex.OrderBy(x => x.Error).ToList();

            for (int i = totalKept; i < output.Length; ++i)
            {
                int errorIndex = errorByIndex[i].Index;
                int outlierIndex = 2;
                output[errorIndex][outlierIndex] += outlierIncrement;
                if (output[errorIndex][outlierIndex] > 1.0)
                {
                    output[errorIndex][outlierIndex] = 1.0;
                }
                //for (int j = 0; j < output[errorByIndex[i].Index].Length; ++j)
                //{

                //    output[errorByIndex[i].Index][j] = 0.8 * output[errorByIndex[i].Index][j];
                //    //if (output[errorByIndex[i].Index][j] > 0.0)
                //    //{
                //    //    output[errorByIndex[i].Index][j] = 0.0;
                //    //}
                //    //else
                //    //{
                //    //    output[errorByIndex[i].Index][j] = 1.0;
                //    //}
                //}
            }
        }
    }
}
