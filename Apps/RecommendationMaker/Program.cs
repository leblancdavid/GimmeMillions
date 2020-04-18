using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using System;
using System.IO;

namespace RecommendationMaker
{
    class Program
    {
        static string _pathToArticles = "../../../../Repository/Articles";
        static string _pathToDictionary = "../../../../Repository/Dictionaries";
        static string _pathToLanguage = "../../../../Repository/Languages";
        static string _pathToStocks = "../../../../Repository/Stocks";
        static string _pathToCache = "../../../../Repository/Cache";
        static string _pathToModels = "../../../../Repository/Models";
        static string _pathToRecommendationConfigs = "../../../../Repository/Recommendations";
        static string _pathToKeys = "../../../../Repository/Keys";

        static void Main(string[] args)
        {
            var datasetService = GetHistoricalFeatureDatasetService();
            var recommendationSystem = new CandlestickStockRecommendationSystem(datasetService, _pathToModels);

            var model = new MLStockFastForestCandlestickModel();
            model.Load(_pathToModels, "ANY_SYMBOL", "AKMBoW1000v1_10d-CandlestickFalse-v2_10d");

            recommendationSystem.AddModel(model);
            recommendationSystem.SaveConfiguration($"{_pathToRecommendationConfigs}/AKMFF-config-v1");
            //recommendationSystem.LoadConfiguration($"{_pathToRecommendationConfigs}/AKMFF-config-v1");

            //var date = new DateTime(2019, 6, 14);
            var date = DateTime.Today;

            var recommendations = recommendationSystem.GetAllRecommendations(date);

            Console.WriteLine("Today's recommended peak stocks:");
            foreach(var r in recommendations)
            {
                Console.WriteLine($"{r.Symbol}: {r.Prediction.Score} ({r.Prediction.Probability})");
            }

            //Console.WriteLine("Loading prediction system...");
            //recommendationSystem.LoadConfiguration($"{_pathToRecommendationConfigs}/KernelFF-config-v1");
            //recommendations = recommendationSystem.GetAllRecommendations(date);

            //Console.WriteLine("Today's recommended stocks:");
            //foreach (var r in recommendations)
            //{
            //    Console.WriteLine($"{r.Symbol}: {r.Prediction.Score} ({r.Prediction.Probability})");
            //}

            Console.ReadKey();
        }

        private static IFeatureDatasetService<FeatureVector> GetBoWFeatureDatasetService(string dictionaryToUse)
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

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            int numArticlesDays = 10;
            return new DefaultFeatureDatasetService(bow, articlesAccess, stocksRepo, numArticlesDays, cache);
        }

        private static HistoricalFeatureDatasetService GetHistoricalFeatureDatasetService()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary("USA");

            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var akmExtractor = new AKMBoWFeatureVectorExtractor(bow, 1000);
            akmExtractor.Load(_pathToModels);

            var stockExtractor = new CandlestickStockFeatureExtractor();

            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesAccess = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            int numArticleDays = 10;
            int numStockSamples = 10;
            FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily;
            return new HistoricalFeatureDatasetService(stockExtractor, akmExtractor, articlesAccess, stocksRepo,
                numArticleDays, numStockSamples, frequencyTimeframe, cache);
        }
    }
}
