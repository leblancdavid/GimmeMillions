using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using Microsoft.Extensions.Logging;
using System;

namespace GimmeMillions.DataAccess.Stocks
{
    public static class RecommendationSystemFactory
    {
        public static IStockRecommendationSystem<FeatureVector> GetCatRecommendationSystem(
            IStockRepository stockRepository,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModel)
        {
            var stocksRepo = new YahooFinanceStockAccessService(stockRepository);
            var extractor = new RawCandlesStockFeatureExtractor();
            var datasetService = new BuySellSignalFeatureDatasetService(extractor, stocksRepo, StockDataPeriod.Day, 15, 20);
            var model = new MLStockRangePredictorModel();
            int filterLength = 3;
            var recommendationSystem = new StockRangeRecommendationSystem(datasetService, stockRecommendationRepository,
                pathToModel, "cat", filterLength, null);

            model.Load(pathToModel);
            recommendationSystem.AddModel(model);

            return recommendationSystem;
        }

        public static IStockRecommendationSystem<FeatureVector> GetDonskoyRecommendationSystem(
            IStockRepository stockRepository,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModel)
        {
            var period = StockDataPeriod.Day;
            var numStockSamples = 100;
            var kernelSize = 9;
            var stocksRepo = new YahooFinanceStockAccessService(stockRepository);
            //var extractor = new RawCandlesStockFeatureExtractor();
            var extractor = new StockIndicatorsFeatureExtractionV2(10,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            var datasetService = new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize);
            var model = new MLStockRangePredictorModel();
            int filterLength = 3;
            var recommendationSystem = new StockRangeRecommendationSystem(datasetService, stockRecommendationRepository,
                pathToModel, "donskoy", filterLength, null);

            model.Load(pathToModel);
            recommendationSystem.AddModel(model);

            return recommendationSystem;
        }

        public static IStockRecommendationSystem<FeatureVector> GetEgyptianMauRecommendationSystem(
            IStockAccessService stocksRepo,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModel, ILogger logger)
        {
            try
            {
                var period = StockDataPeriod.Day;
                var numStockSamples = 80;
                var kernelSize = 9;
                //var extractor = new RawCandlesStockFeatureExtractor();
                var extractor = new StockIndicatorsFeatureExtractionV2(12,
                    numStockSamples,
                    (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                    (int)(numStockSamples * 0.8), 5,
                    (int)(numStockSamples * 0.8), 5,
                    (int)(numStockSamples * 0.8), 5,
                    false);
                var datasetService = new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                    period, numStockSamples, kernelSize);
                var model = new MLStockRangePredictorModel();
                int filterLength = 3;
                var recommendationSystem = new StockRangeRecommendationSystem(datasetService, stockRecommendationRepository,
                    pathToModel, "egyptianMau", filterLength, logger);

                model.Load(pathToModel);
                recommendationSystem.AddModel(model);

                return recommendationSystem;
            }
            catch(Exception ex)
            {
                logger?.LogError(ex.Message);
                throw ex;
            }
        }

        public static IStockRecommendationSystem<FeatureVector> GetDonskoyCryptoRecommendationSystem(
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModel,
            string secret, string key, string passphrase)
        {
            var period = StockDataPeriod.Day;
            var numStockSamples = 100;
            var kernelSize = 9;
            var stocksRepo = new CoinbaseApiAccessService(secret, key, passphrase);
            //var extractor = new RawCandlesStockFeatureExtractor();
            var extractor = new StockIndicatorsFeatureExtractionV2(10,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            var datasetService = new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize);
            var model = new MLStockRangePredictorModel();
            int filterLength = 3;
            var recommendationSystem = new StockRangeRecommendationSystem(datasetService, stockRecommendationRepository,
                pathToModel, "donskoy", filterLength, null);

            model.Load(pathToModel);
            recommendationSystem.AddModel(model);

            return recommendationSystem;
        }

        private static IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetService(
            IStockRepository stockRepository,
           int numStockSamples = 40,
           int stockOutputPeriod = 3,
           bool includeComposites = false)
        {
            var stocksRepo = new YahooFinanceStockAccessService(stockRepository);

            var indictatorsExtractor = new StockIndicatorsFeatureExtraction(normalize: false);

            return new CandlestickStockFeatureDatasetService(indictatorsExtractor, stocksRepo,
                StockDataPeriod.Day, numStockSamples, includeComposites, null, false);
        }

        private static IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetServiceV2(
            IStockRepository stockRepository,
          int numStockSamples = 40,
          int stockOutputPeriod = 3)
        {
            var stocksRepo = new YahooFinanceStockAccessService(stockRepository);
            var indictatorsExtractor = new StockIndicatorsFeatureExtractionV2(10,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);

            return new CandlestickStockWithFuturesFeatureDatasetService(indictatorsExtractor, stocksRepo,
                StockDataPeriod.Day, numStockSamples);
        }
    }
}