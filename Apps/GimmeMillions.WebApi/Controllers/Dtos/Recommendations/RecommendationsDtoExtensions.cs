using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Recommendations;
using System.Linq;

namespace GimmeMillions.WebApi.Controllers.Dtos.Recommendations
{
    public static class RecommendationsDtoExtensions
    {
        public static StockDataDto ToDto(this StockData stockData)
        {
            return new StockDataDto()
            {
                Date = stockData.Date,
                Symbol = stockData.Symbol,
                Open = stockData.Open,
                High = stockData.High,
                Low = stockData.Low,
                Close = stockData.Close,
                AdjustedClose = stockData.AdjustedClose,
                PreviousClose = stockData.PreviousClose,
                Volume = stockData.Volume
            };
        }

        public static StockRecommendationDto ToDto(this StockRecommendation stockRecommendation)
        {
            return new StockRecommendationDto()
            {
                Date = stockRecommendation.Date,
                Symbol = stockRecommendation.Symbol,
                SystemId = stockRecommendation.SystemId,
                Sentiment = stockRecommendation.Sentiment,
                Confidence = stockRecommendation.Confidence,
                Prediction = stockRecommendation.Prediction,
                LowPrediction = stockRecommendation.LowPrediction,
                PreviousClose = stockRecommendation.PreviousClose,
                PredictedPriceTarget = stockRecommendation.PredictedPriceTarget,
                PredictedLowTarget = stockRecommendation.PredictedLowTarget,
                LastData = stockRecommendation.LastData.ToDto()
            };
        }

        public static StockRecommendationHistoryDto ToDto(this StockRecommendationHistory history)
        {
            return new StockRecommendationHistoryDto()
            {
                Symbol = history.Symbol,
                SystemId = history.SystemId,
                HistoricalData = history.HistoricalData.Select(x => x.ToDto()).ToList(),
                LastUpdated = history.LastUpdated,
                LastRecommendation = history.LastRecommendation.ToDto()
            };
        }
    }
}
