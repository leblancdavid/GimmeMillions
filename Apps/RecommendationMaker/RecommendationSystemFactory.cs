using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
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
