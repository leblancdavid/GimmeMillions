using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Accord;
using GimmeMillions.Domain.ML.Binary;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTrainer
{
    class Program
    {
        static string _pathToArticles = "../../../../Repository/Articles";
        static string _pathToDictionary = "../../../../Repository/Dictionaries";
        static string _pathToLanguage = "../../../../Repository/Languages";
        static string _pathToStocks = "../../../../Repository/Stocks";
        static string _pathToCache = "../../../../Repository/Cache";
        //static string _pathToRecommendationConfigs = "../../../../Repository/Recommendations";
        static string _pathToModels = "../../../../Repository/Models";
        static string _pathToKeys = "../../../../Repository/Keys";
        static void Main(string[] args)
        {
            string dictionaryToUse = "USA";
            var stocks = new string[] { "ET",
            "T", "PFE", "PBR", "GILD", "CSCO", "NOK", "MGM", "XOM", "HAL", "JPM", "CMCSA", "MS", "CVX", "PCG", "MRK",
            "V", "EBAY", "WMT", "LUV", "NKE", "JNJ", "SYF", "HLT", "CVS"};

            //var stocks = new string[] { "F","INTC", "MSFT", "ATVI", "VZ", "S", "INVA", "LGND", "LXRX", "XBI",
            // "IWM", "AMZN", "GOOG", "AAPL", "RAD", "WBA", "DRQ", "CNX", "BOOM", "FAST", "DAL", "ZNH", "ARNC",
            // "AAL", "ORCL", "AMD", "MU", "INFY", "CAJ", "HPQ", "PSA-PH", "DRE", "NLY", "MPW", "C", "WFC",
            //"HSBC", "BAC", "RY", "AXP", "FB", "DIS", "BHP", "BBL", "DD", "GOLD", "DUK", "EXC", "FE", "EIX",
            //"CMS", "MCD", "SBUX", "LOW", "HMC", "HD", "GM", "ROST", "BBY", "MAR", "KO", "PEP", "GIS", "GE", "ET",
            //"T", "PFE", "PBR", "GILD", "CSCO", "NOK", "MGM", "XOM", "HAL", "JPM", "CMCSA", "MS", "CVX", "PCG", "MRK",
            //"V", "EBAY", "WMT", "LUV", "NKE", "JNJ", "SYF", "HLT", "CVS"};
            var datasetService = GetBoWFeatureDatasetService(dictionaryToUse);

            var recommendationSystem = new StockRecommendationSystem(datasetService, _pathToModels);

            var startDate = new DateTime(2010, 1, 1);
            var endDate = DateTime.Today.AddDays(-1.0);

            double totalCount = 0.0, totalAccuracy = 0.0;
            foreach(var stock in stocks)
            {
                //var model = new MLStockBinaryFastForestModel();
                Console.WriteLine($"-=== Loading training data for {stock} ===-");
                //var model = new MLStockFastForestModel();
                //model.Parameters.FeatureSelectionRank = 500;
                //model.Parameters.NumCrossValidations = 10;
                //model.Parameters.NumOfTrees = 2000;
                //model.Parameters.NumOfLeaves = 20;
                //model.Parameters.MinNumOfLeaves = 20;

                var model = new MLStockKernelEstimationSvmModel();
                model.Parameters.FeatureSelectionRank = 500;
                model.Parameters.NumCrossValidations = 5;
                model.Parameters.NumOfTrees = 2000;
                model.Parameters.NumOfLeaves = 20;
                model.Parameters.MinNumOfLeaves = 20;
                model.Parameters.NumIterations = 20;
                model.Parameters.KernelRank = 300;

                //var model = new MLStockKnnBruteForceModel();
                //model.Parameters.FeatureSelectionRank = 50000;

                ////var model = new MLStock();
                //model.Parameters.FeatureSelectionRank = 500;
                //model.Parameters.NumCrossValidations = 10;
                //model.Parameters.NumOfTrees = 2000;
                //model.Parameters.NumOfLeaves = 20;
                //model.Parameters.MinNumOfLeaves = 20;

                //var model = new MLStockPcaSvmModel();
                //model.Parameters.FeatureSelectionRank = 500;
                //model.Parameters.NumCrossValidations = 10;
                //model.Parameters.PcaRank = 250;

                var dataset = datasetService.GetTrainingData(stock, startDate, endDate);

                var filteredDataset = dataset.Value;
                int numTestExamples = 0;

                var testSet = filteredDataset.Skip(filteredDataset.Count() - numTestExamples);
                var trainingSet = filteredDataset.Take(filteredDataset.Count() - numTestExamples);


                Console.WriteLine($"-=== Training {stock} ===-");
                //Console.WriteLine($"Num Features: { model.Parameters.FeatureSelectionRank}");
                //Console.WriteLine($"Pca Rank: { model.Parameters.PcaRank}");

                Console.WriteLine($"Num Features: { model.Parameters.FeatureSelectionRank}");
               // Console.WriteLine($"Number of Trees: { model.Parameters.NumOfTrees} \t Number of Leaves: { model.Parameters.NumOfLeaves}");

                Stopwatch stopwatch = Stopwatch.StartNew();
                var trainingResult = model.Train(trainingSet, 0.0);
                stopwatch.Stop();

                Console.WriteLine($"-=== Training done ===-");
                Console.WriteLine($"Training time: {stopwatch.ElapsedMilliseconds / 60000.0}");
                if (trainingResult.IsFailure)
                {
                    Console.WriteLine($"Training failed: {trainingResult.Error}");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"-=== Results {stock} ===-");
                Console.WriteLine($"Accuracy: {trainingResult.Value.Accuracy} \t Area under PR curve: {trainingResult.Value.AreaUnderPrecisionRecallCurve}");
                Console.WriteLine($"Positive Precision: {trainingResult.Value.PositivePrecision} \t Positive Recall: {trainingResult.Value.PositiveRecall}");
                Console.WriteLine($"Negative Precision: {trainingResult.Value.NegativePrecision} \t Negative Recall: {trainingResult.Value.NegativeRecall}");

                Console.WriteLine($"-=== Saving Model {stock} ===-");
                model.Save(_pathToModels);

                //Console.WriteLine($"-=== Testing Model  {stock} ===-");
                //double accuracy = 0.0;
                //foreach (var testExample in testSet)
                //{
                //    var prediction = model.Predict(testExample.Input);
                //    if ((prediction.PredictedLabel && testExample.Output.PercentDayChange > 0) ||
                //         (!prediction.PredictedLabel && testExample.Output.PercentDayChange <= 0))
                //    {
                //        accuracy++;
                //        //Console.WriteLine($"Good! Probability: {prediction.Probability}");

                //    }
                //    else
                //    {
                //        //Console.WriteLine($"Bad! Probability: {prediction.Probability}");
                //    }
                //}

                //Console.WriteLine($"Test Accuracy {stock}: {accuracy / numTestExamples}");
                //totalAccuracy += accuracy;
                //totalCount += numTestExamples;
                //Console.WriteLine($"Running Accuracy: {totalAccuracy / totalCount}");
            }
            
            Console.WriteLine($"-=========================================================================================-");
            //Console.WriteLine($"Total Accuracy: {totalAccuracy / totalCount}");

            Console.ReadKey();
        }

        private static IFeatureDatasetService GetBoWFeatureDatasetService(string dictionaryToUse)
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary(dictionaryToUse);

            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesAccess = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache(_pathToCache);

            return new DefaultFeatureDatasetService(bow, articlesAccess, stocksRepo, cache);
        }
    }
}
