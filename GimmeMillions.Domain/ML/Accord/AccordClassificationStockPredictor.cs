using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.Performance;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;
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
    public class AccordClassificationStockPredictor : IStockPredictionModel<FeatureVector>
    {
        private int _rank = 1000;
        private int _pcaRank = 200;
        private IDataTransformer _filterTransformer;
        //private IDataTransformer _supervisedNormalizer;
        private PrincipalComponentAnalysis _pca;
        //private RandomForest _rt;
        private SupportVectorMachine<Gaussian> _svm;
        // private LogisticRegression _regression;
        //private DecisionTree _tree;
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
            //var transformed = _pca.Transform(
            //            inputData);

            //var transformed = _pca.Transform(
            //        _filterTransformer.Transform(
            //            inputData)).Concat(input.CandlestickData).ToArray();
            var transformed = input.Data;


            return new StockPrediction()
            {
                PredictedLabel = _svm.Decide(transformed),
                Score = _svm.Score(transformed),
                Probability = _svm.Probability(transformed)
            };
        }

        public Result Save(string pathToModel)
        {
            throw new NotImplementedException();
        }

        public Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction)
        {
            var datasetList = dataset.ToList();
            var newsInput = datasetList.Select(x => x.Input.Data).ToArray();
            _pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true,
                NumberOfOutputs = _pcaRank
            };

            //_filterTransformer = new FilterFeaturesDataTransformer(_rank);
            //_filterTransformer.Fit(newsInput);
            //var filteredData = _filterTransformer.Transform(newsInput);
            //_pca.NumberOfOutputs = _pcaRank;
            //var transform = _pca.Learn(filteredData);
            //var pcaTransformed = _pca.Transform(filteredData);

            
            //_svm = trainer.Learn(pcaTransformed, outputs);
            var trainingData = new double[newsInput.Length][];
            for (int i = 0; i < newsInput.Length; ++i)
            {
                //trainingData[i] = pcaTransformed[i].Concat(datasetList[i].Input.CandlestickData).ToArray();
                trainingData[i] = datasetList[i].Input.Data;
            }

            var outputs = datasetList.Select(x => x.Output.PercentDayChange > 0.0m ? 1 : 0).ToArray();
            // Now, we can create the sequential minimal optimization teacher
            var learn = new SequentialMinimalOptimization<Gaussian>()
            {
                UseComplexityHeuristic = false,
                UseKernelEstimation = false
            };
            //And then we can obtain a trained SVM by calling its Learn method
            _svm = learn.Learn(trainingData, outputs);

            //var learner = new IterativeReweightedLeastSquares<LogisticRegression>()
            //{
            //    Tolerance = 1e-4,  // Let's set some convergence parameters
            //    Iterations = 100,  // maximum number of iterations to perform
            //    Regularization = 0
            //};

            //// Now, we can use the learner to finally estimate our model:
            //_regression = learner.Learn(trainingData, outputs);

            // And we can use the C4.5 for learning:
            //C45Learning teacher = new C45Learning();

            // Finally induce the tree from the data:
            //_tree = teacher.Learn(trainingData, outputs);


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
    }
}
