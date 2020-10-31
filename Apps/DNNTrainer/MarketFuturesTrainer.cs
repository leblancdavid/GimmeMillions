﻿using GimmeMillions.DataAccess.Stocks;
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
   
    public class MarketFuturesTrainer
    {
        private IStockRepository _stockRepository;
        public MarketFuturesTrainer(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;

        }
        public void Train(string modelFile)
        {
            var datasetService = GetCandlestickFeatureDatasetServiceV3(20, 5, 5);

            var model = new MLStockRangePredictorModel();

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            //var filter = new DefaultDatasetFilter(maxPercentHigh: 10.0m, maxPercentLow: 10.0m);
            trainingData.AddRange(datasetService.GetTrainingData("DIA", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("SPY", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^RUT", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^RUI", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^DJT", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^DJU", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^DJI", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^GSPC", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^IXIC", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("^NDX", null, true).Value);

            var averageGrowth = trainingData.Average(x => x.Output.PercentChangeFromPreviousClose);

            var trainingResults = model.Train(trainingData, 0.0, new PercentDayChangeOutputMapper(averageGrowth));
            model.Save(modelFile);
        }

        private IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetServiceV3(
           int numStockSamples = 40,
           int timesampling = 5,
           int kernelSize = 9)
        {
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            var extractor = new RawStockFeatureExtractor();
            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                numStockSamples, kernelSize);
        }
    }
}
