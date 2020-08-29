using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendationMaker
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
            string pathToModels)
        {
            int numStockSamples = 60, outputPeriod = 5;
            bool usesCompositeIndex = true;
            var datasetService = GetCandlestickFeatureDatasetService(stockRepository, numStockSamples, outputPeriod, usesCompositeIndex);
            var recommendationSystem = new CandlestickStockRecommendationSystem(datasetService, pathToModels);

            string modelEncoding = "Indicators-MACD(32,16,12,7)VWAP(12,7)RSI(12,7)CMF(24,7),nFalse-v1_60d-5p_withComposite";
            var model = new MLStockFastForestCandlestickModel();
            model.Load(pathToModels, "ANY_SYMBOL", modelEncoding);
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
    }
}
