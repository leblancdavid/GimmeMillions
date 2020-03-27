using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.Performance;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Binary;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Accord
{
    public class AccordClassificationStockPredictor : IStockPredictionModel
    {
        private int _rank = 1000;
        private int _pcaRank = 500;
        private IDataTransformer _filterTransformer;
        private IDataTransformer _supervisedNormalizer;
        private PrincipalComponentAnalysis _pca;
        private RandomForest _rt;

        public string StockSymbol { get; set; }

        public bool IsTrained { get; set; }

        public string Encoding { get; set; }

        public Result Load(string pathToModel, string symbol, string encoding)
        {
            throw new NotImplementedException();
        }

        public StockPrediction Predict(FeatureVector input)
        {
            var inputData = input.Data.Select(x => (double)x).ToArray();
            var transformed = _pca.Transform(
                 _supervisedNormalizer.Transform(
                    _filterTransformer.Transform(
                        inputData)));

            //var transformed = _pca.Transform(
            //        _filterTransformer.Transform(
            //            inputData));

            //var transformed = _lda.Transform(
            //        _filterTransformer.Transform(
            //            inputData));
            return new StockPrediction()
            {
                PredictedLabel = _rt.Decide(transformed) > 0,
                //Score = _rt.Score(transformed),
                //Probability = _rt.Probability(transformed)
            };

            //return new StockPrediction()
            //{
            //    PredictedLabel = _svm.Decide(transformed),
            //    Score = _svm.Score(transformed),
            //    Probability = _svm.Probability(transformed)
            //};
        }

        public Result Save(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            // Create a new Sequential Minimal Optimization (SMO) learning 
            // algorithm and estimate the complexity parameter C from data
            //var trainer = new SequentialMinimalOptimization<Gaussian>()
            //{
            //    UseComplexityHeuristic = true,
            //    UseKernelEstimation = true // estimate the kernel from the data
            //};

            var trainer = new RandomForestLearning()
            {
                NumberOfTrees = 100
            };


            var outputs = dataset.Select(x => x.Output.PercentDayChange >= 0 ? 1 : 0).ToArray();
            var inputs = GetSupervisedNormalizedFeatures(
                GetFilteredFeatures(dataset.Select(x => Array.ConvertAll(x.Input.Data, y => (double)y)).ToArray(), outputs, _rank),
                outputs);

            //var inputs = GetFilteredFeatures(dataset.Select(x => Array.ConvertAll(x.Input.Data, y => (double)y)).ToArray(), outputs, _rank);
            
            _pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true
            };
            var transform = _pca.Learn(inputs);
            _pca.NumberOfOutputs = _pcaRank;
            var pcaTransformed = _pca.Transform(inputs);

            //_lda = new LinearDiscriminantAnalysis();
            //_lda.Learn(inputs, outputs);
            //var ldaTransformed = _lda.Transform(inputs);

            // Compute the cross-validation
            // Create a new Cross-validation algorithm passing the data set size and the number of folds
            //var crossvalidation = new CrossValidation<RandomForestLearning, double[]>()
            //{
            //    K = 5,
            //    //Learner = (s) => new SequentialMinimalOptimization<Gaussian, double[]>()
            //    //{
            //    //    UseKernelEstimation = true,
            //    //    UseComplexityHeuristic = true,
            //    //},
            //    Learner = (s) => new RandomForestLearning()
            //    {
            //        NumberOfTrees = 100
            //    },
            //    Loss = (expected, actual, p) => new ZeroOneLoss(expected).Loss(actual),
            //    Stratify = false
            //};
            //crossvalidation.ParallelOptions.MaxDegreeOfParallelism = 4;
            //var results = crossvalidation.Learn(pcaTransformed, outputs);

            //_svm = trainer.Learn(pcaTransformed, outputs);
            _rt = trainer.Learn(pcaTransformed, outputs);

            // If desired, compute an aggregate confusion matrix for the validation sets:
            //GeneralConfusionMatrix gcm = results.ToConfusionMatrix(pcaTransformed, outputs);

            return Result.Ok(new ModelMetrics()
            {
                //Accuracy = gcm.Accuracy,
                //PositivePrecision = gcm.Precision[0],
                //PositiveRecall = gcm.Recall[0],
                //NegativePrecision = gcm.Precision[1],
                //NegativeRecall = gcm.Recall[1]
            });

        }
        private (double Variance, int Index)[] GetAbsoluteVariance(double[][] inputs, int[] outputs)
        {

            if (!inputs.Any())
                throw new Exception($"Input features for the FeatureSelectorEstimator contains no elements");

            int featureLength = inputs.First().Length;
            var average = new double[featureLength];

            for (int i = 0; i < featureLength; ++i)
            {
                average[i] = 0.0;
                for (int j = 0; j < outputs.Length; ++j)
                {
                    average[i] += inputs[j][i];
                }
            }

            var variance = new (double Variance, int Index)[average.Length];
            for (int i = 0; i < featureLength; ++i)
            {
                average[i] /= outputs.Length;

                for (int j = 0; j < outputs.Length; ++j)
                {
                    variance[i].Variance += Math.Pow(inputs[j][i] - average[i], 2.0);
                }

                variance[i].Variance = Math.Sqrt(variance[i].Variance / outputs.Length);
                variance[i].Index = i;
            }

            return variance;
        }

        private (double FeatureDifference, int Index)[] GetAbsoluteDifference(double[][] inputs, int[] outputs)
        {

            if (!inputs.Any())
                throw new Exception($"Input features for the FeatureSelectorEstimator contains no elements");

            int featureLength = inputs.First().Length;
            var positiveScore = new double[featureLength];
            var negativeScore = new double[featureLength];
            var positiveVar = new double[featureLength];
            var negativeVar = new double[featureLength];
            double negativeTotal = outputs.Sum(x => x > 0 ? 1.0 : 0.0),
                positiveTotal = outputs.Sum(x => x < 1 ? 1.0 : 0.0);

            for (int i = 0; i < featureLength; ++i)
            {
                //Initialize with a 1
                positiveScore[i] = 0.0;
                negativeScore[i] = 0.0;
                for (int j = 0; j < outputs.Length; ++j)
                {
                    if (outputs[j] > 0)
                    {
                        //positiveScore[i] += inputs[j][i];
                        if (inputs[j][i] > 0.0)
                            positiveScore[i]++;
                    }
                    else
                    {
                        //negativeScore[i] += inputs[j][i];
                        if (inputs[j][i] > 0.0)
                            negativeScore[i]++;
                    }
                }
            }

            var p = new (double FeatureDifference, int Index)[positiveScore.Length];
            for (int i = 0; i < featureLength; ++i)
            {
                positiveScore[i] /= positiveTotal;
                negativeScore[i] /= negativeTotal;

                positiveVar[i] = 0.0;
                negativeVar[i] = 0.0;

                for (int j = 0; j < outputs.Length; ++j)
                {
                    if (outputs[j] > 0)
                    {
                        positiveVar[i] += Math.Pow(inputs[j][i] - positiveScore[i], 2.0);
                    }
                    else
                    {
                        negativeVar[i] += Math.Pow(inputs[j][i] - negativeScore[i], 2.0);
                    }
                }

                positiveVar[i] = Math.Sqrt(positiveVar[i] / positiveTotal);
                negativeVar[i] = Math.Sqrt(negativeVar[i] / negativeTotal);

                //p[i] = ((float)(positiveScore[i] - negativeScore[i]) / (positiveVar[i] + negativeVar[i]), i);
                //p[i] = ((negativeScore[i] - positiveScore[i]) / (positiveVar[i] + negativeVar[i]), i);
                //p[i] = ((float)Math.Abs(negativeAvg[i] - positiveAvg[i]) / (positiveVar[i] + negativeVar[i]), i);
                //p[i] = (negativeScore[i] - positiveScore[i], i);
                //p[i] = ((negativeScore[i]), i);
                //p[i] = (positiveScore[i], i);
                p[i] = (positiveScore[i] - negativeScore[i], i);
                //p[i] = (Math.Abs(positiveScore[i] - negativeScore[i]), i);
                if (double.IsNaN(p[i].FeatureDifference))
                {
                    p[i].FeatureDifference = 0.0;
                }

            }

            return p;
        }

        private int[] GetFeatureSelectionIndices(double[][] inputs, int[] outputs, int rank)
        {
            var differences = GetAbsoluteDifference(inputs, outputs);
            var orderedDifferences = differences.OrderByDescending(x => x.FeatureDifference).ToList();

            //var variances = GetAbsoluteVariance(inputs, outputs); 
            //var orderedDifferences = variances.OrderByDescending(x => x.Variance).ToList();

            //var indicesToKeep = orderedDifferences.Take(rank).Select(x => x.Index);
            int skipUnimportantWords = 25;
            var indicesToKeep = orderedDifferences.Skip(skipUnimportantWords).Take(rank).Select(x => x.Index);
            return indicesToKeep.ToArray();
        }

        private double[][] GetFilteredFeatures(double[][] inputs, int[] outputs, int rank)
        {
            _filterTransformer = new FilterFeaturesDataTransformer(GetFeatureSelectionIndices(inputs, outputs, rank));
            return _filterTransformer.Transform(inputs);
        }

        private double[][] GetSupervisedNormalizedFeatures(double[][] inputs, int[] outputs)
        {
            _supervisedNormalizer = new SupervisedNormalizationDataTransformer(ComputeDataStatistics(inputs, outputs));
            return _supervisedNormalizer.Transform(inputs);
        }

        private (double pMean, double pStdev, double nMean, double nStdev)[] ComputeDataStatistics(double[][] inputs, int[] outputs)
        {
            if (!inputs.Any())
                throw new Exception($"Input features contains no elements");


            int featureLength = inputs.First().Length;
            var statistics = new (double pMean, double pStdev, double nMean, double nStdev)[featureLength];
            double negativeTotal = outputs.Sum(x => x > 0 ? 1.0 : 0.0),
                positiveTotal = outputs.Sum(x => x < 1 ? 1.0 : 0.0);

            for (int i = 0; i < featureLength; ++i)
            {
                for (int j = 0; j < inputs.Length; ++j)
                {
                    if (outputs[j] > 0)
                    {
                        statistics[i].pMean += inputs[j][i];
                    }
                    else
                    {
                        statistics[i].nMean += inputs[j][i];
                    }
                }
            }

            for (int i = 0; i < featureLength; ++i)
            {
                statistics[i].pMean /= positiveTotal;
                statistics[i].nMean /= negativeTotal;

                for (int j = 0; j < inputs.Length; ++j)
                {
                    if (outputs[j] > 0)
                    {
                        statistics[i].pStdev += Math.Pow(inputs[j][i] - statistics[i].pMean, 2.0);
                    }
                    else
                    {
                        statistics[i].nStdev += Math.Pow(inputs[j][i] - statistics[i].nMean, 2.0);
                    }
                }

                statistics[i].pStdev = Math.Sqrt(statistics[i].pStdev / positiveTotal);
                statistics[i].nStdev = Math.Sqrt(statistics[i].nStdev / negativeTotal);
            }

            return statistics;
        }
    }
}
