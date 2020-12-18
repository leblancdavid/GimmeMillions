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
    class EgyptianMauModelTrainer
    {
        private IStockRepository _stockRepository;
        private string _tdAccessFile;
        public EgyptianMauModelTrainer(IStockRepository stockRepository, string tdAccessFile)
        {
            _stockRepository = stockRepository;
            _tdAccessFile = tdAccessFile;
        }
        public void Train(string pathToModels)
        {
            //var datasetService = GetRawFeaturesBuySellSignalDatasetService(20, 9);
            var datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(StockDataPeriod.Day, 100, 9);
            var model = new MLStockRangePredictorModel();

            int numSamples = 20000;
            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            var filter = new MarketHoursUtcFilter();
            filter = null;
            trainingData.AddRange(datasetService.GetTrainingData("DIA", filter, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("SPY", filter, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("QQQ", filter, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("RUT", filter, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("RUI", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("NDX", null, true));

            model.Train(trainingData, 0.0, new SignalOutputMapper());
            model.Save(pathToModels + "\\Futures");

            //trainingData.Clear();
            //var stockFilter = new DefaultStockFilter(
            //        maxPercentHigh: 50.0m,
            //    maxPercentLow: 50.0m,
            //    minPrice: 2.0m,
            //    maxPrice: 50.0m,
            //    minVolume: 1000.0m);
            //trainingData.AddRange(datasetService.GetAllTrainingData(stockFilter, true, numSamples));
            //model.Train(trainingData, 0.0, new SignalOutputMapper());
            //model.Save(pathToModels + "\\Stocks");
        }

        public void TrainCrypto(string pathToModels, StockDataPeriod period, IStockAccessService stockAccessService)
        {
            var datasetService = GetCoinbaseIndicatorFeaturesBuySellSignalDatasetService(stockAccessService, period, 100, 15);
            var model = new MLStockRangePredictorModel();

            int numSamples = 200;

            string modelName = pathToModels + $"\\Crypto{period.ToString()}";

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(datasetService.GetTrainingData("BTC-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("ETH-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("LTC-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("XRP-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("BCH-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("EOS-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("DASH-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("OXT-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("MKR-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("XLM-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("LINK-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("ATOM-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("ETC-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("XTZ-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("REP-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("DAI-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("KNC-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("OMG-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("ZRX-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("ALGO-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("BAND-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("LRC-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("YFI-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("UNI-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("REN-USD", null, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("BAL-USD", null, true, numSamples));

            model.Train(trainingData, 0.0, new SignalOutputMapper());
            model.Save(modelName);
        }

        private IFeatureDatasetService<FeatureVector> GetCoinbaseIndicatorFeaturesBuySellSignalDatasetService(
            IStockAccessService stockAccessService,
           StockDataPeriod period,
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            //var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            //var extractor = new RawCandlesStockFeatureExtractor();
            var extractor = new StockIndicatorsFeatureExtractionV2(10,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            return new BuySellSignalFeatureDatasetService(extractor, stockAccessService,
                period, numStockSamples, kernelSize);
        }

        private IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
           StockDataPeriod period,
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            var stocksRepo = new TDAmeritradeStockAccessService(new TDAmeritradeApiClient(_tdAccessFile));
            var extractor = new StockIndicatorsFeatureExtractionV2(10,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize);
        }
    
    }
}
