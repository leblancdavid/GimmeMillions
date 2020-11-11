using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNTrainer
{
    public class CryptoCatModelTrainer
    {
        private IStockRepository _stockRepository;
        public CryptoCatModelTrainer(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;

        }
        public void Train(string modelFile)
        {
            var datasetService = GetRawFeaturesBuySellSignalDatasetService(15, 20);
            var model = new MLStockRangePredictorModel();

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(datasetService.GetTrainingData("BTC-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("ETH-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("USDT-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("XRP-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("LINK-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("BCH-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("BNB-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("LTC-USD", null, true).Value);
            //trainingData.AddRange(datasetService.GetTrainingData("DOT2-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("DOT1-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("ADA-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("BSV-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("EOS-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("XMR-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("TRX-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("XLM-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("XMR-USD", null, true).Value);
            trainingData.AddRange(datasetService.GetTrainingData("NEO-USD", null, true).Value);

            var trainingResults = model.Train(trainingData, 0.0, new SignalOutputMapper());
            model.Save(modelFile);
        }

        private IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetServiceV3(
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            var extractor = new RawCandlesStockFeatureExtractor();
            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                numStockSamples, kernelSize);
        }

        private IFeatureDatasetService<FeatureVector> GetRawFeaturesBuySellSignalDatasetService(
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            var extractor = new RawCandlesStockFeatureExtractor();
            //var extractor = new RawPriceStockFeatureExtractor();

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                numStockSamples, kernelSize);
        }
    }
}
