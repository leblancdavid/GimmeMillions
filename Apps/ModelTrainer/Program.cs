using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Binary;
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
        static void Main(string[] args)
        {
            string dictionaryToUse = "FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries";
            string stock = "AMZN";
            var datasetService = GetBoWFeatureDatasetService(dictionaryToUse);

            var model = new MLStockBinaryFastForestModel(stock);
            double bestPR = 0.0f;
            MLStockBinaryFastForestModel bestModel = null;

            var startDate = new DateTime(2000, 1, 1);
            var endDate = new DateTime(2004, 6, 1);
            var dataset = datasetService.GetTrainingData(stock, startDate, endDate);
            var testSplit = 0.1;
            for(int numFeatures = 500; numFeatures < 5000; numFeatures += 500)
            {
                for (int pca = 1; pca < 20; pca++)
                {
                    model.Parameters.NumOfTrees = 20;
                    model.Parameters.NumOfLeaves = 5;
                    model.Parameters.PcaRank = pca;
                    model.Parameters.FeatureSelectionRank = numFeatures;
                    model.Parameters.NumIterations = 10;

                    Console.WriteLine($"-=== Training ===-");
                    Console.WriteLine($"Num Features: { model.Parameters.FeatureSelectionRank} \t PCA: { model.Parameters.PcaRank}");
                    Console.WriteLine($"Number of Trees: { model.Parameters.NumOfTrees} \t Number of Leaves: { model.Parameters.NumOfLeaves}");
                    Console.WriteLine($"Pca Rank: {model.Parameters.PcaRank}");
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    var trainingResult = model.Train(dataset.Value, testSplit);
                    stopwatch.Stop();

                    Console.WriteLine($"-=== Training done ===-");
                    Console.WriteLine($"Training time: {stopwatch.ElapsedMilliseconds / 60000.0}");
                    if (trainingResult.IsFailure)
                    {
                        Console.WriteLine($"Training failed: {trainingResult.Error}");
                        continue;
                    }

                    Console.WriteLine($"-=== Results ===-");
                    Console.WriteLine($"Accuracy: {trainingResult.Value.Accuracy} \t Area under PR curve: {trainingResult.Value.AreaUnderPrecisionRecallCurve}");
                    if (trainingResult.Value.AreaUnderPrecisionRecallCurve > bestPR)
                    {
                        bestPR = trainingResult.Value.AreaUnderPrecisionRecallCurve;
                        bestModel = model;
                    }

                    Console.WriteLine($"-=========================================================================================-");
                }
            }
            

            Console.ReadLine();
        }

        private static IFeatureDatasetService GetBoWFeatureDatasetService(string dictionaryToUse)
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary(dictionaryToUse);

            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var stocksRepo = new StockDataRepository(_pathToStocks); 
            
            var cache = new FeatureJsonCache(_pathToCache);

            return new DefaultFeatureDatasetService(bow, articlesRepo, stocksRepo, cache);
        }
    }
}
