using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.SQLDataAccess
{
    public class SQLStockDataRepository : IStockRepository
    {
        private readonly DbContextOptions<GimmeMillionsContext> _dbContextOptions;
        public SQLStockDataRepository(DbContextOptions<GimmeMillionsContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public IEnumerable<string> GetSymbols()
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            return context.StockDatas.Select(x => x.Symbol);
        }

        public IEnumerable<StockData> GetStocks(string symbol, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            if (timeframe == FrequencyTimeframe.Daily)
            {
                return GetDailyStocks(symbol);
            }
            else
            {
                var dailyStocks = GetDailyStocks(symbol);
                return GetWeeklyStocks(symbol, dailyStocks.ToList());
            }
        }

        public Result<StockData> GetStock(string symbol, DateTime date, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            var stockFound = GetStocks(symbol, date, date, timeframe);
            if (!stockFound.Any())
            {
                return Result.Failure<StockData>($"Unable to retrieve stock {symbol} for date {date.ToString("MM/dd/yyyy")}");
            }
            return Result.Ok(stockFound.First());
        }

        public Result AddOrUpdateStock(StockData stockData)
        {
            try
            {
                var context = new GimmeMillionsContext(_dbContextOptions);
                var existingStockData = context.StockDatas.FirstOrDefault(x => x.Symbol == stockData.Symbol && x.Date == stockData.Date);
                if (existingStockData == null)
                {
                    context.StockDatas.Add(stockData);
                }
                else
                {
                    existingStockData.AdjustedClose = stockData.AdjustedClose;
                    existingStockData.Close = stockData.Close;
                    existingStockData.High = stockData.High;
                    existingStockData.Low = stockData.Low;
                    existingStockData.Open = stockData.Open;
                    existingStockData.PreviousClose = stockData.PreviousClose;
                }
                context.SaveChanges();
                return Result.Ok();
            }
            catch(Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        public void Delete(string symbol)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            context.StockDatas.RemoveRange(context.StockDatas.Where(x => x.Symbol == symbol));
            context.SaveChanges();
        }

        private IEnumerable<StockData> GetDailyStocks(string symbol)
        {
            var context = new GimmeMillionsContext(_dbContextOptions);
            var stocks = context.StockDatas.Where(x => x.Symbol == symbol);
            return stocks.OrderBy(x => x.Date);
        }

        private IEnumerable<StockData> GetWeeklyStocks(string symbol, List<StockData> dailyStocks)
        {
            var weeklyStocks = new List<StockData>();
            if (!dailyStocks.Any())
                return weeklyStocks;
            int i = 0;
            StockData startingDay = dailyStocks.First();
            while (i < dailyStocks.Count - 1 &&
                dailyStocks[i].Date.DayOfWeek >= startingDay.Date.DayOfWeek)
            {
                startingDay = dailyStocks[i];
                ++i;
            }


            while (i < dailyStocks.Count - 1)
            {
                startingDay = dailyStocks[i];
                var endingDay = startingDay;
                decimal high = 0.0m;
                decimal low = decimal.MaxValue;
                decimal volume = 0.0m;
                while (i < dailyStocks.Count &&
                dailyStocks[i].Date.DayOfWeek >= endingDay.Date.DayOfWeek)
                {
                    endingDay = dailyStocks[i];
                    if (endingDay.Low < low)
                    {
                        low = endingDay.Low;
                    }
                    if (endingDay.High > high)
                    {
                        high = endingDay.High;
                    }
                    volume += endingDay.Volume;
                    ++i;
                }

                weeklyStocks.Add(new StockData(symbol, startingDay.Date,
                    startingDay.Open, high, low,
                    endingDay.Close, endingDay.AdjustedClose, volume,
                    startingDay.PreviousClose));
            }

            return weeklyStocks;
        }

        public IEnumerable<StockData> GetStocks(string symbol, DateTime start, DateTime end, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            return GetStocks(symbol, timeframe).Where(x => x.Date >= start && x.Date <= end);
        }


        public IEnumerable<StockData> GetStocks(string symbol, int timeLength)
        {
            var dailyStocks = GetDailyStocks(symbol).ToList();
            if (timeLength <= 1)
            {
                return dailyStocks;
            }

            var periodStocks = new List<StockData>();
            if (!dailyStocks.Any())
                return periodStocks;

            decimal previousClose = dailyStocks.First().Close;
            for (int i = 0; i < dailyStocks.Count() - timeLength; ++i)
            {
                var currentDay = dailyStocks[i];
                decimal high = 0.0m;
                decimal low = decimal.MaxValue;
                decimal volume = 0.0m;
                decimal close = 0.0m, adjustedClose = 0.0m;
                for (int j = i; j < i + timeLength; ++j)
                {
                    var stockDay = dailyStocks[j];
                    if (stockDay.Low < low)
                    {
                        low = stockDay.Low;
                    }
                    if (stockDay.High > high)
                    {
                        high = stockDay.High;
                    }
                    volume += stockDay.Volume;
                    close = stockDay.Close;
                    adjustedClose = stockDay.AdjustedClose;
                }

                periodStocks.Add(new StockData(symbol, currentDay.Date,
                    currentDay.Open, high, low,
                    close, adjustedClose, volume,
                    previousClose));
                previousClose = currentDay.Close;
            }

            return periodStocks;
        }

       
    }
}
