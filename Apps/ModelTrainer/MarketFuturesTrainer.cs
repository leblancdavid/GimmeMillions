﻿using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System.Collections.Generic;

namespace ModelTrainer
{

    public class MarketFuturesTrainer
    {
        private IStockRepository _stockRepository;
        public MarketFuturesTrainer(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;

        }
        public void TrainFutures(string pathToModels)
        {
            var datasetService = GetRawFeaturesBuySellSignalDatasetService(15, 20);
            //var datasetService = GetRawFeaturesCandlestickDatasetService(20);

            var model = new MLStockRangePredictorModel();

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            //var filter = new DefaultDatasetFilter(maxPercentHigh: 10.0m, maxPercentLow: 10.0m);
            trainingData.AddRange(datasetService.GetTrainingData("DIA", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("SPY", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^RUT", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^RUI", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^DJT", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^DJU", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^DJI", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^GSPC", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^IXIC", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^NDX", null, true));

            var trainingResults = model.Train(trainingData, 0.0, new SignalOutputMapper());
            model.Save(pathToModels);
        }

       

        private IFeatureDatasetService<FeatureVector> GetRawFeaturesBuySellSignalDatasetService(
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            var extractor = new RawCandlesStockFeatureExtractor();
            //var extractor = new RawPriceStockFeatureExtractor();

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                StockDataPeriod.Day, numStockSamples, kernelSize);
        }
    }
}