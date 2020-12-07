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
        IStockRecommendationRepository RecommendationRepository { get; }
        string SystemId { get; }
        Result LoadConfiguration(string configurationFile);
        Result SaveConfiguration(string configurationFile);
        IEnumerable<StockRecommendation> RunRecommendationsForToday(IStockFilter filter = null,
            int keepTop = 10, bool updateStockHistory = false);
        IEnumerable<StockRecommendation> RunRecommendations(DateTime date, IStockFilter filter = null, int keepTop = 10);
        IEnumerable<StockRecommendation> RunAllRecommendationsForToday(IStockFilter filter = null);
        IEnumerable<StockRecommendation> RunAllRecommendations(DateTime date, IStockFilter filter = null);
        IEnumerable<StockRecommendation> RunRecommendationsFor(IEnumerable<string> symbols, DateTime date, 
            IStockFilter filter = null);
        Result<StockRecommendation> RunRecommendationsFor(string symbol, DateTime date);
        IEnumerable<StockRecommendation> GetRecommendationsForToday(int keep = 10);
        IEnumerable<StockRecommendation> GetRecommendations(DateTime date, int keep = 10);
        IEnumerable<StockRecommendation> GetRecommendations(IEnumerable<string> symbols, DateTime date);
        Result<StockRecommendation> GetRecommendation(DateTime date, string symbol);
    }
}
