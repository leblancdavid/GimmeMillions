using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Recommendations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Database
{
    public class SQLStockRecommendationHistoryRepository : IStockRecommendationHistoryRepository
    {
        private readonly DbContextOptions<GimmeMillionsContext> _dbContextOptions;
        private readonly ILogger _logger;
        private object _saveLock = new object();
        public SQLStockRecommendationHistoryRepository(DbContextOptions<GimmeMillionsContext> dbContextOptions,
            ILogger logger)
        {
            _dbContextOptions = dbContextOptions;
            _logger = logger;
        }

        public Result AddOrUpdateRecommendation(StockRecommendation recommendation)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var symbolHistory = context.RecommendationHistories.FirstOrDefault(x => x.Symbol == recommendation.Symbol && x.SystemId == recommendation.SystemId);

                if(symbolHistory == null)
                {
                    symbolHistory = new StockRecommendationHistory(recommendation.SystemId, recommendation.Symbol,
                        new List<StockRecommendation>() { recommendation });

                    context.RecommendationHistories.Add(symbolHistory);
                }
                else
                {
                    symbolHistory.AddOrUpdateRecommendation(recommendation);
                    context.RecommendationHistories.Update(symbolHistory);
                }

                var lastPrediction = context.LastRecommendations
                    .Include(x => x.LastData)
                    .FirstOrDefault(x => x.LastData.Symbol == recommendation.Symbol && 
                                    x.SystemId == recommendation.SystemId);
                if(lastPrediction != null)
                {
                    if(lastPrediction.Date < recommendation.Date)
                    {
                        context.LastRecommendations.Remove(lastPrediction);
                        context.LastRecommendations.Add(recommendation);
                    }
                }
                else
                {
                    context.LastRecommendations.Add(recommendation);
                }

                lock (_saveLock)
                {
                    context.SaveChanges();
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result.Failure(ex.Message);
            }
        }

        public Result AddOrUpdateRecommendationHistory(StockRecommendationHistory history)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var symbolHistory = context.RecommendationHistories.FirstOrDefault(x => x.Symbol == history.Symbol && x.SystemId == history.SystemId);

                if (symbolHistory != null)
                {
                    symbolHistory.HistoricalDataStr = history.HistoricalDataStr;

                    symbolHistory.LoadData();
                    symbolHistory.LastUpdated = history.LastUpdated;
                }
                else
                {
                    context.RecommendationHistories.Add(history);
                }

                var r = history.LastRecommendation;
                if(r != null)
                {
                    var lastPrediction = context.LastRecommendations
                        .Include(x => x.LastData)
                        .FirstOrDefault(x => x.LastData.Symbol == history.Symbol && x.SystemId == history.SystemId);
                    if (lastPrediction != null)
                    {

                        if (lastPrediction.Date < r.Date)
                        {
                            context.LastRecommendations.Remove(lastPrediction);
                            context.LastRecommendations.Add(r);
                        }
                    }
                    else
                    {
                        context.LastRecommendations.Add(r);
                    }

                }

                lock (_saveLock)
                {
                    context.SaveChanges();
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result.Failure(ex.Message);
            }
        }

        public Result AddOrUpdateRecommendations(IEnumerable<StockRecommendation> recommendations)
        {
            try
            {
                if(!recommendations.Any())
                {
                    return Result.Success();
                }

                var context = new GimmeMillionsContext(_dbContextOptions);

                var symbolHistory = context.RecommendationHistories.FirstOrDefault(x => x.Symbol == recommendations.First().Symbol);

                if (symbolHistory == null)
                {
                    symbolHistory = new StockRecommendationHistory(recommendations.First().SystemId, recommendations.First().Symbol,
                        recommendations);

                    context.RecommendationHistories.Add(symbolHistory);
                }
                else
                {
                    foreach(var r in recommendations)
                    {
                        symbolHistory.AddOrUpdateRecommendation(r);
                    }

                    context.RecommendationHistories.Update(symbolHistory);
                }

                lock (_saveLock)
                {
                    context.SaveChanges();
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result.Failure(ex.Message);
            }
        }

        public Result<StockRecommendation> GetStockRecommendation(string systemId, string symbol, DateTime dateTime)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            var history = context.RecommendationHistories.FirstOrDefault(x => x.Symbol == symbol &&
                    x.SystemId == systemId);
            if (history != null)
            {
                var stock = history.HistoricalData.FirstOrDefault(x => x.Date.Date == dateTime.Date);
                if(stock != null)
                {
                    return Result.Success(stock);
                }
            }

            return Result.Failure<StockRecommendation>($"No recommendation found for {symbol} for sytem {systemId} on {dateTime.ToString()}");
        }

        public Result<StockRecommendationHistory> GetStockRecommendationHistory(string systemId, string symbol)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            var history = context.RecommendationHistories.FirstOrDefault(x => x.Symbol == symbol &&
                    x.SystemId == systemId);

            if (history != null)
            {
                history.LoadData();
                return Result.Success(history);
            }

            return Result.Failure<StockRecommendationHistory>($"No recommendation history found for {symbol} for sytem {systemId}");
        }

        public IEnumerable<StockRecommendation> GetStockRecommendations(string systemId, string symbol)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            var history = context.RecommendationHistories.FirstOrDefault(x => x.Symbol == symbol &&
                    x.SystemId == systemId);
            if (history != null)
            {
                return history.HistoricalData;
            }

            return new List<StockRecommendation>();
        }

        public IEnumerable<StockRecommendation> GetStockRecommendations(string systemId, DateTime dateTime)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            var recommendations = new List<StockRecommendation>();
            var history = context.RecommendationHistories.Where(x => x.SystemId == systemId);
            if (history != null)
            {
                foreach(var h in history)
                {
                    var r = h.HistoricalData.FirstOrDefault(x => x.Date.Date == dateTime);
                    if(r != null)
                    {
                        recommendations.Add(r);
                    }
                }
            }

            return recommendations;
        }

        public IEnumerable<StockRecommendation> GetStockRecommendations(string systemId)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            return context.LastRecommendations.Where(x => x.SystemId == systemId);
        }

        public void RemoveAll()
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);
                context.LastRecommendations.RemoveRange(context.LastRecommendations);
                context.RecommendationHistories.RemoveRange(context.RecommendationHistories);

                context.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
