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
    public class DonskoyModelTrainer
    {
        private IStockRepository _stockRepository;
        public DonskoyModelTrainer(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;

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
            trainingData.AddRange(datasetService.GetTrainingData("^RUT", filter, true, numSamples));
            trainingData.AddRange(datasetService.GetTrainingData("^RUI", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^DJT", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^DJU", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^DJI", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^GSPC", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^IXIC", null, true));
            trainingData.AddRange(datasetService.GetTrainingData("^NDX", null, true));

            model.Train(trainingData, 0.0, new SignalOutputMapper());
            model.Save(pathToModels + "\\Futures");

            trainingData.Clear();
            var stockFilter = new DefaultStockFilter(
                    maxPercentHigh: 50.0m,
                maxPercentLow: 50.0m,
                minPrice: 2.0m,
                maxPrice: 50.0m,
                minVolume: 1000.0m);
            trainingData.AddRange(datasetService.GetAllTrainingData(stockFilter, true, numSamples));
            model.Train(trainingData, 0.0, new SignalOutputMapper());
            model.Save(pathToModels + "\\Stocks");
        }

        private IFeatureDatasetService<FeatureVector> GetRawFeaturesBuySellSignalDatasetService(
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            var stocksRepo = new AlpacaStockAccessService(_stockRepository);
            //var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            var extractor = new RawCandlesStockFeatureExtractor();

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                StockDataPeriod.FifteenMinute, numStockSamples, kernelSize);
        }

        private IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
           StockDataPeriod period,
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            //var stocksRepo = new AlpacaStockAccessService(_stockRepository);
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            //var extractor = new RawCandlesStockFeatureExtractor();
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