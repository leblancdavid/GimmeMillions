﻿using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Logging;
using GimmeMillions.Domain.ML.Accord;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNTrainer
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
            //var datasetService = GetCandlestickFeatureDatasetService(60, 5, true);
            var datasetService = GetCandlestickFeatureDatasetServiceV2(200, 5, false);
            var logFile = "logs/log";
            Directory.CreateDirectory("logs");
            var loggers = new List<ILogger>()
            {
                new FileLogger(logFile),
                new ConsoleLogger()
            };
            //var model = new DNNRegressionStockPredictor(loggers);
            //var model = new MLStockFastForestCandlestickModel();
            var model = new MLStockFastForestCandlestickModelV2();
            model.Parameters.NumCrossValidations = 2;
            model.Parameters.NumOfTrees = 2000;
            model.Parameters.NumOfLeaves = 200;
            model.Parameters.MinNumOfLeaves = 10;

            //var endTrainingData = new DateTime(2019, 1, 1);
            var endTrainingData = DateTime.Today;
            var dataset = datasetService.GetAllTrainingData(new DateTime(2015, 1, 1), endTrainingData, 0.5m, 100000.0m, false);
            //var dataset = datasetService.GetTrainingData("AMZN", new DateTime(2001, 1, 30), endTrainingData).Value;

            var trainingResults = model.Train(dataset, 0.1);
            model.Save(_pathToModels);
        }

        private static IFeatureDatasetService<FeatureVector> GetHistoricalFeatureDatasetService(string dictionaryToUse,
            int numArticleDays = 10,
            int numStockSamples = 10, 
            FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily,
            bool includeComposites = false)
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary(dictionaryToUse);

            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var akmExtractor = new AKMBoWFeatureVectorExtractor(bow, 1000);
            akmExtractor.Load(_pathToModels);

            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesAccess = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks));

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            var candlestickExtractor = new CandlestickStockFeatureExtractor();

            return new HistoricalFeatureDatasetService(candlestickExtractor, akmExtractor, articlesAccess, stocksRepo,
                numArticleDays, numStockSamples, frequencyTimeframe, includeComposites, cache);
        }

        private static IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetService(
           int numStockSamples = 40,
           int stockOutputPeriod = 3,
           bool includeComposites = false)
        {
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks));

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            //var candlestickExtractor = new CandlestickStockFeatureExtractor();
            //use default values for meow!
            var indictatorsExtractor = new StockIndicatorsFeatureExtraction(normalize: false);

            return new CandlestickStockFeatureDatasetService(indictatorsExtractor, stocksRepo,
                numStockSamples, stockOutputPeriod, includeComposites, cache, false);
        }

        private static IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetServiceV2(
           int numStockSamples = 40,
           int stockOutputPeriod = 3,
           bool includeFutures = false)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite($"Data Source=C:/Databases/gm.db");
            var context = new GimmeMillionsContext(optionsBuilder.Options);
            context.Database.Migrate();

            var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var stocksRepo = new YahooFinanceStockAccessService(new DefaultStockRepository(stockSqlDb));

            //var candlestickExtractor = new CandlestickStockFeatureExtractor();
            //use default values for meow!
            //var indictatorsExtractor = new StockIndicatorsFeatureExtraction(normalize: false);
            var indictatorsExtractor = new StockIndicatorsFeatureExtractionV2(10, 
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5, 
                false);

            return new CandlestickStockWithFuturesFeatureDatasetService(indictatorsExtractor, stocksRepo,
                numStockSamples, stockOutputPeriod);
        }
    }
}
