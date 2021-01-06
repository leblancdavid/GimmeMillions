using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Database
{
    public class SQLStockRecommendationRepository : IStockRecommendationRepository
    {
        private readonly DbContextOptions<GimmeMillionsContext> _dbContextOptions;
        private object _saveLock = new object();
        public SQLStockRecommendationRepository(DbContextOptions<GimmeMillionsContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public Result AddOrUpdateRecommendation(StockRecommendation recommendation)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var stocks = context.StockRecommendations.Where(x => x.Symbol == recommendation.Symbol &&
                    x.Date.Date == recommendation.Date.Date && 
                    x.SystemId == recommendation.SystemId);
                if (stocks != null && stocks.Count() > 0)
                {
                    context.StockRecommendations.RemoveRange(stocks);
                }

                context.StockRecommendations.Add(recommendation);

                lock (_saveLock)
                {
                    context.SaveChanges();
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public Result UpdateRecommendation(StockRecommendation recommendation)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);

                var stock = context.StockRecommendations.FirstOrDefault(x => x.Id == recommendation.Id);
                if (stock != null)
                {
                    context.StockRecommendations.Update(recommendation);
                    context.SaveChanges();
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public Result<StockRecommendation> GetStockRecommendation(string systemId, string symbol, DateTime dateTime)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            var stock = context.StockRecommendations.FirstOrDefault(x => x.Symbol == symbol &&
                    x.Date.Date == dateTime.Date &&
                    x.SystemId == systemId);
            if(stock == null)
            {
                return Result.Failure<StockRecommendation>($"No recommendation found for {symbol} for sytem {systemId} on {dateTime.ToString()}");
            }
            return Result.Success<StockRecommendation>(stock);
        }

        public IEnumerable<StockRecommendation> GetStockRecommendations(string systemId, string symbol)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            return context.StockRecommendations.Where(x => x.Symbol == symbol &&
                    x.SystemId == systemId);
        }

        public IEnumerable<StockRecommendation> GetStockRecommendations(string systemId, DateTime dateTime)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);

            return context.StockRecommendations.Where(x =>
                    x.Date.Date == dateTime.Date &&
                    x.SystemId == systemId);
        }

        public void RemoveAll()
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            context.StockRecommendations.RemoveRange(context.StockRecommendations);
        }
    }
}
