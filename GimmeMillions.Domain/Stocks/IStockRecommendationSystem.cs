using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRecommendationSystem<TFeature>
        where TFeature : FeatureVector
    {
        Result LoadConfiguration(string configurationFile);
        Result SaveConfiguration(string configurationFile);
        IEnumerable<StockRecommendation> GetRecommendationsForToday(int keepTop = 10, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetRecommendations(DateTime date, int keepTop = 10, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetAllRecommendationsForToday(bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetAllRecommendations(DateTime date, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> GetRecommendationsFor(IEnumerable<string> symbols, DateTime date, bool updateStockHistory = false);
    }
}
