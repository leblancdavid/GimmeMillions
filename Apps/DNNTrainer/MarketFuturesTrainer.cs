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
using System.Linq;

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
            var datasetService = GetCandlestickFeatureDatasetServiceV3(50, 5, 11);

            var model = new MLStockFastForestCandlestickModel();
            model.Parameters.NumCrossValidations = 2;
            model.Parameters.NumOfTrees = 2000;
            model.Parameters.NumOfLeaves = 20;
            model.Parameters.MinNumOfLeaves = 5;
            //var model = new MLStockRangePredictorModel();

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            //var filter = new DefaultDatasetFilter(maxPercentHigh: 10.0m, maxPercentLow: 10.0m);
            trainingData.AddRange(datasetService.GetTrainingData("DIA", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("SPY", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^RUT", null, true).Value);
            //trainingData.AddRange(datasetService.GetTrainingData("AMZN", null, true).Value);
            //trainingData.AddRange(datasetService.GetTrainingData("GOOG", null, true).Value);
            //trainingData.AddRange(datasetService.GetTrainingData("AAPL", null, true).Value);

            var trainingResults = model.Train(trainingData, 0.1, new PercentDayChangeOutputMapper());
            model.Save(modelFile);

            var diaSamples = datasetService.GetFeatures("DIA").Where(x => x.Date > new DateTime(2020, 1, 1));
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"C:\\Stocks\\dia-test.txt"))
            {
                foreach (var sample in diaSamples)
                {
                    var prediction = model.Predict(sample);
                    //file.WriteLine($"{sample.Date}\t{prediction.Sentiment}\t{prediction.PredictedHigh}\t{prediction.PredictedLow}");
                    file.WriteLine($"{sample.Date}\t{prediction.Probability}");
                }
            }
        }

        private IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetServiceV3(
           int numStockSamples = 40,
           int timesampling = 5,
           int kernelSize = 9)
        {
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);

            var indictatorsExtractor = new StockIndicatorsFeatureExtractionV3(timesampling,
                numStockSamples, //BOLL
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5, //MACD
                (int)(numStockSamples), 5, //VWAP
                (int)(numStockSamples), 5, //RSI
                (int)(numStockSamples), 5, //CMF
                5);
            return new BuySellSignalFeatureDatasetService(indictatorsExtractor, stocksRepo,
                numStockSamples, kernelSize);
        }
    }
}
