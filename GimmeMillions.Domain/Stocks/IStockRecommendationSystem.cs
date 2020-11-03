using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks.Filters;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRecommendationSystem<TFeature>
        where TFeature : FeatureVector
    {
        Result LoadConfiguration(string configurationFile);
        Result SaveConfiguration(string configurationFile);
        IEnumerable<StockRecommendation> GetRecommendationsForToday(IStockFilter filter = null,
            int keepTop = 10, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetRecommendations(DateTime date, IStockFilter filter = null, int keepTop = 10, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetAllRecommendationsForToday(IStockFilter filter = null, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetAllRecommendations(DateTime date, IStockFilter filter = null, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetRecommendationsFor(IEnumerable<string> symbols, DateTime date, 
            IStockFilter filter = null, bool updateStockHistory = false);
    }
}
