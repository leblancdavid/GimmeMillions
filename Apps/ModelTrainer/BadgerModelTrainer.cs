using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using System;

namespace DNNTrainer
{
    public class BadgerModelTrainer
    {
        string _pathToArticles = "../../../../Repository/Articles";
        string _pathToDictionary = "../../../../Repository/Dictionaries";
        string _pathToLanguage = "../../../../Repository/Languages";
        string _pathToStocks = "../../../../Repository/Stocks";
        string _pathToCache = "../../../../Repository/Cache";
        string _pathToModels = "../../../../Repository/Models";
        string _pathToKeys = "../../../../Repository/Keys";

        public void Train()
        {
            //var datasetService = GetCandlestickFeatureDatasetService(60, 5, true);
            var datasetService = GetCandlestickFeatureDatasetServiceV2(200, 5, false);
            //var model = new DNNRegressionStockPredictor(loggers);
            //var model = new MLStockFastForestCandlestickModel();
            var model = new MLStockFastForestCandlestickModelV2();
            model.Parameters.NumCrossValidations = 2;
            model.Parameters.NumOfTrees = 2000;
            model.Parameters.NumOfLeaves = 200;
            model.Parameters.MinNumOfLeaves = 10;

            //var endTrainingData = new DateTime(2019, 1, 1);
            var endTrainingData = DateTime.Today;

            decimal minPrice = 1.0m;
            decimal maxPrice = 20.0m;
            decimal minVol = 500000m;
            decimal maxHighPercent = 40.0m; //max high will filter out huge gains due to news
            var dataset = datasetService.GetAllTrainingData(new DefaultStockFilter(new DateTime(2000, 1, 30), endTrainingData,
                minPrice, maxPrice, minVol, maxPercentHigh: maxHighPercent), false);
            var trainingResults = model.Train(dataset, 0.0, null);
            model.Save(_pathToModels);

            //var dataset = datasetService.GetTrainingData("F").Value.ToList();
            //model.Load(_pathToModels, "ANY_SYMBOL", "Indicators-Boll(200)MACD(160,80,60,5)VWAP(160,5)RSI(160,5)CMF(160,5),nFalse-v2_200d-5p_withFutures");

            //foreach(var data in dataset)
            //{
            //    var prediction = model.Predict(data.Input);
            //}
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

            //var indictatorsExtractor = new NormalizedVolumePriceActionFeatureExtractor(numStockSamples);

            return new CandlestickStockWithFuturesFeatureDatasetService(indictatorsExtractor, stocksRepo,
                StockDataPeriod.Day, numStockSamples);
        }
    }
}