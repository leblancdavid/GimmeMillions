using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Features.Extractors;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

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

        public static IStockRecommendationSystem<FeatureVector> GetHimalayanRecommendationSystem(
            IStockAccessService stocksRepo,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModel, ILogger logger)
        {
            try
            {
                var period = StockDataPeriod.Day;
                var numStockSamples = 80;
                var kernelSize = 9;
                var extractor = new MultiStockFeatureExtractor(new List<IFeatureExtractor<StockData>>
                {
                    new FibonacciStockFeatureExtractor(),
                    new TrendStockFeatureExtractor(numStockSamples / 2),
                    new StockIndicatorsFeatureExtractionV3(12,
                    numStockSamples,
                    (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                    (int)(numStockSamples * 0.8), 5,
                    (int)(numStockSamples * 0.8), 5,
                    (int)(numStockSamples * 0.8), 5,
                    false)
                });
                
                var datasetService = new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                    period, numStockSamples, kernelSize);
                var model = new DeepLearningStockRangePredictorModel();
                int filterLength = 3;
                var recommendationSystem = new StockRangeRecommendationSystem(datasetService, stockRecommendationRepository,
                    pathToModel, "himalayan", filterLength, logger);

                model.Load(pathToModel);
                recommendationSystem.AddModel(model);

                return recommendationSystem;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                throw ex;
            }
        }

        public static IStockRecommendationSystem<FeatureVector> GetJavaneseRecommendationSystem(
            IStockAccessService stocksRepo,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModel, ILogger logger)
        {
            try
            {
                var period = StockDataPeriod.Day;
                var numStockSamples = 200;
                var kernelSize = 9;
                var extractor = new MultiStockFeatureExtractor(new List<IFeatureExtractor<StockData>>
                {
                    //new SupportResistanceStockFeatureExtractor(),
                    //new FibonacciStockFeatureExtractor(),
                    new MACDHistogramFeatureExtraction(10),
                    new RSIFeatureExtractor(10),
                    new VWAPFeatureExtraction(10),
                    new CMFFeatureExtraction(10),
                    new BollingerBandFeatureExtraction(10),
                    new TrendStockFeatureExtractor(10),
                    new SimpleMovingAverageFeatureExtractor(20)
                });

                var datasetService = new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                    period, numStockSamples, kernelSize);
                var model = new MLStockRangePredictorModelV2();
                int filterLength = 3;
                var recommendationSystem = new StockRangeRecommendationSystem(datasetService, stockRecommendationRepository,
                    pathToModel, "javanese", filterLength, logger);

                model.Load(pathToModel);
                recommendationSystem.AddModel(model);

                return recommendationSystem;
            }
            catch (Exception ex)
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
    }
}