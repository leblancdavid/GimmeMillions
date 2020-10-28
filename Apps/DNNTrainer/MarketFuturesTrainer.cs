using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Logging;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;

namespace DNNTrainer
{
    public class PercentDayChangeOutputMapper : ITrainingOutputMapper
    {
        public bool GetBinaryValue(StockData stockData)
        {
            return stockData.PercentChangeFromPreviousClose > 0.0m;
        }

        public float GetOutputValue(StockData stockData)
        {
            return (float)stockData.PercentChangeFromPreviousClose;
        }
    };
    public class MarketFuturesTrainer
    {
        private IStockRepository _stockRepository;
        public MarketFuturesTrainer(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;

        }
        public void Train(string modelFile)
        {
            //var datasetService = GetCandlestickFeatureDatasetService(60, 5, true);
            var datasetService = GetCandlestickFeatureDatasetServiceV2(200, 10, false);

            var model = new MLStockFastForestCandlestickModelV2();
            //model.Parameters.NumCrossValidations = 2;
            //model.Parameters.NumOfTrees = 2000;
            //model.Parameters.NumOfLeaves = 200;
            //model.Parameters.MinNumOfLeaves = 100;

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            //var filter = new DefaultDatasetFilter(maxPercentHigh: 10.0m, maxPercentLow: 10.0m);
            trainingData.AddRange(datasetService.GetTrainingData("DIA", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("SPY", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^RUT", null, true).Value);
            var trainingResults = model.Train(trainingData, 0.1, new PercentDayChangeOutputMapper());
            model.Save(modelFile);
        }

        private IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetServiceV2(
           int numStockSamples = 40,
           int stockOutputPeriod = 3,
           bool includeFutures = false)
        {
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);

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
