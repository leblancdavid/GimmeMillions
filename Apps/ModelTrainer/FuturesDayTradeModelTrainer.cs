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
            //var datasetService = GetRawFeaturesBuySellSignalDatasetService(20, 9);
            var datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(period, 12, 80, 9);
            var model = new MLStockRangePredictorModel();

            int numSamples = 20000;
            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(datasetService.GetTrainingData("DIA", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("SPY", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("RUT", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("AAPL", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("MSFT", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("AMZN", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("GOOG", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("GOOGL", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("FB", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("BABA", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("TSLA", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("V", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("JPM", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("JNJ", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("WMT", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("MA", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("NVDA", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("DIS", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("PYPL", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("PG", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("BAC", null, true, numSamples));

            model.Train(trainingData, 0.1, new SignalOutputMapper());
            model.Save(modelName);
        }

        private IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
            StockDataPeriod period,
            int timeSampling = 10,
            int numStockSamples = 40,
            int kernelSize = 9)
        {
            var stocksRepo = new TDAmeritradeStockAccessService(new TDAmeritradeApiClient(_tdAccessFile), _stockSymbolsRepository);
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

        private IFeatureDatasetService<FeatureVector> GetRawFeaturesBuySellSignalDatasetService(
            int numStockSamples = 40,
            int kernelSize = 9)
        {
            var stocksRepo = new TDAmeritradeStockAccessService(new TDAmeritradeApiClient(_tdAccessFile), _stockSymbolsRepository);
            var extractor = new RawCandlesStockFeatureExtractor();
            //var extractor = new RawPriceStockFeatureExtractor();

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                StockDataPeriod.Day, numStockSamples, kernelSize);
        }
    }
}
