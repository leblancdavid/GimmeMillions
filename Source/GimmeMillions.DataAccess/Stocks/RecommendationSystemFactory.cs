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
        /// <summary>
        /// The "Aadvark" recommendation system is the first version of the prediction engine
        /// It uses 60 days of stock samples, includes the DJI, GSPC, IXIC indexes, predicting
        /// a 5 day output. The stock sample features are encoded using:
        ///  MACD(32,16,12,7),
        ///  VWAP(12,7)
        ///  RSI(12,7)
        ///  CMF(24,7)
        /// </summary>
        /// <param name="stockRepository">
        ///  The repositor where the stock data is stored
        /// </param>
        /// <param name="pathToModels">
        ///  Path to where the expected model lives
        /// </param>
        /// <returns></returns>
        public static IStockRecommendationSystem<FeatureVector> GetAadvarkRecommendationSystem(
            IStockRepository stockRepository,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModels)
        {
            int numStockSamples = 60, outputPeriod = 5;
            bool usesCompositeIndex = true;
            var datasetService = GetCandlestickFeatureDatasetService(stockRepository, numStockSamples, outputPeriod, usesCompositeIndex);
            var recommendationSystem = new CandlestickStockRecommendationSystem(datasetService, stockRecommendationRepository, pathToModels, "aadvark");

            //string modelEncoding = "Indicators-MACD(32,16,12,7)VWAP(12,7)RSI(12,7)CMF(24,7),nFalse-v1_60d-5p_withComposite";
            var model = new MLStockFastForestCandlestickModel();
            model.Load(pathToModels);
            recommendationSystem.AddModel(model);

            return recommendationSystem;
        }

        public static IStockRecommendationSystem<FeatureVector> GetBadgerRecommendationSystem(
            IStockRepository stockRepository,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModels)
        {
            int numStockSamples = 200, outputPeriod = 5;
            var datasetService = GetCandlestickFeatureDatasetServiceV2(stockRepository, numStockSamples, outputPeriod);
            var recommendationSystem = new CandlestickStockRecommendationSystem(datasetService, stockRecommendationRepository, pathToModels, "badger");

            //string modelEncoding = "Indicators-Boll(200)MACD(160,80,60,5)VWAP(160,5)RSI(160,5)CMF(160,5),nFalse-v2_200d-5p_withFutures";
            var model = new MLStockFastForestCandlestickModelV2();
            model.Load(pathToModels);
            recommendationSystem.AddModel(model);

            return recommendationSystem;
        }

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