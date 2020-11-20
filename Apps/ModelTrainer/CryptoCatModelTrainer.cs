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
            trainingData.AddRange(datasetService.GetTrainingData("BTC-USD", null, true));
            //trainingData.AddRange(datasetService.GetTrainingData("ETH-USD", null, true).Value);
            //trainingData.AddRange(datasetService.GetTrainingData("USDT-USD", null, true).Value);
            //trainingData.AddRange(datasetService.GetTrainingData("XRP-USD", null, true).Value);
            //trainingData.AddRange(datasetService.GetTrainingData("LINK-USD", null, true).Value);

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
                StockDataPeriod.Day, numStockSamples, kernelSize);
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
