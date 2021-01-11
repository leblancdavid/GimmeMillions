using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.Domain.Stocks.Recommendations
{
    public interface IStockRecommendationHistoryRepository : IStockRecommendationRepository
    {
        Result AddOrUpdateRecommendations(IEnumerable<StockRecommendation> recommendation);
        Result AddOrUpdateRecommendationHistory(StockRecommendationHistory history);
        Result<StockRecommendationHistory> GetStockRecommendationHistory(string systemId, string symbol);
    }
}
