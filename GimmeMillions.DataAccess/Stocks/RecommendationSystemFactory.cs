using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;

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

            string modelEncoding = "Indicators-MACD(32,16,12,7)VWAP(12,7)RSI(12,7)CMF(24,7),nFalse-v1_60d-5p_withComposite";
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

            string modelEncoding = "Indicators-Boll(200)MACD(160,80,60,5)VWAP(160,5)RSI(160,5)CMF(160,5),nFalse-v2_200d-5p_withFutures";
            var model = new MLStockFastForestCandlestickModelV2();
            model.Load(pathToModels);
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
                numStockSamples, stockOutputPeriod, includeComposites, null, false);
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
                numStockSamples, stockOutputPeriod);
        }
    }
}
