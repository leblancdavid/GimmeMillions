using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Logging;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;

namespace DNNTrainer
{
    public class MarketFuturesTrainer
    {
        public MarketFuturesTrainer()
        {

        }
        public void Train(string modelFile)
        {
            //var datasetService = GetCandlestickFeatureDatasetService(60, 5, true);
            var datasetService = GetCandlestickFeatureDatasetServiceV2(200, 5, false);

            var model = new MLStockFastForestCandlestickModelV2();
            model.Parameters.NumCrossValidations = 2;
            model.Parameters.NumOfTrees = 2000;
            model.Parameters.NumOfLeaves = 200;
            model.Parameters.MinNumOfLeaves = 100;

            //var endTrainingData = new DateTime(2019, 1, 1);
            var endTrainingData = DateTime.Today;

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(datasetService.GetTrainingData("DIA", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("SPY", null, true).Value); 
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^RUT", null, true).Value);
            var trainingResults = model.Train(trainingData, 0.0, null);
            model.Save(modelFile);
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
