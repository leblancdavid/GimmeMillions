using GimmeMillions.DataAccess.Clients.TDAmeritrade;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelTrainer
{
    public class FuturesDayTradeModelTrainer
    {
        private IStockRepository _stockRepository;
        private IStockSymbolsRepository _stockSymbolsRepository;
        private string _tdAccessFile;
        public FuturesDayTradeModelTrainer(IStockRepository stockRepository,
            IStockSymbolsRepository stockSymbolsRepository,
            string tdAccessFile)
        {
            _stockSymbolsRepository = stockSymbolsRepository;
            _stockRepository = stockRepository;
            _tdAccessFile = tdAccessFile;
        }
        public void Train(string modelName, StockDataPeriod period)
        {
            //var datasetService = GetRawFeaturesBuySellSignalDatasetService(period, 50, 9);
            var datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(period, 12, 80, 13);
            var model = new MLStockRangePredictorModel();

            int numSamples = 10000;
            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(datasetService.GetTrainingData("DIA", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("SPY", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("RUT", null, true, numSamples));

            model.Train(trainingData, 0.1, new SignalOutputMapper());
            model.Save(modelName);
        }

        private IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
            StockDataPeriod period,
            int timeSampling = 10,
            int numStockSamples = 40,
            int kernelSize = 9)
        {
            var stocksRepo = new AlpacaStockAccessService();
            var extractor = new StockIndicatorsFeatureExtractionV2(timeSampling,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize);
        }

        private IFeatureDatasetService<FeatureVector> GetRawFeaturesBuySellSignalDatasetService(StockDataPeriod period,
            int numStockSamples = 40,
            int kernelSize = 9)
        {
            var stocksRepo = new AlpacaStockAccessService();
            var extractor = new RawCandlesStockFeatureExtractor();
            //var extractor = new RawPriceStockFeatureExtractor();

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize);
        }
    }
}
