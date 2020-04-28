using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GimmeMillions.DataAccess.Stocks
{
    public class StockDataRepository : IStockRepository
    {
        private readonly string _pathToStocks;
        public StockDataRepository(string pathToStocks)
        {
            _pathToStocks = pathToStocks;
        }

        public Result<StockData> GetStock(string symbol, DateTime date, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            var stockFound = GetStocks(symbol, date, date, timeframe);
            if(!stockFound.Any())
            {
                return Result.Failure<StockData>($"Unable to retrieve stock {symbol} for date {date.ToString("MM/dd/yyyy")}");
            }
            return Result.Ok(stockFound.First());
        }

        public IEnumerable<StockData> GetStocks(string symbol, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            if(timeframe == FrequencyTimeframe.Daily)
            {
                return GetDailyStocks(symbol);
            }
            else
            {
                return GetWeeklyStocks(symbol);
            }
        }

        private IEnumerable<StockData> GetDailyStocks(string symbol)
        {
            var stocks = new List<StockData>();
            string fileName = $"{_pathToStocks}/{symbol}";
            if (!File.Exists(fileName))
            {
                return stocks;
            }

            // Read the file and display it line by line.  
            var file = new StreamReader(fileName);
            string line;
            file.ReadLine(); //First line is the header
            StockData previous = null;
            while ((line = file.ReadLine()) != null)
            {
                var fields = line.Split(',');
                DateTime date;
                decimal open, high, low, close, adjustedClose, volume;
                if (DateTime.TryParse(fields[0], out date) &&
                    decimal.TryParse(fields[1], out open) &&
                    decimal.TryParse(fields[2], out high) &&
                    decimal.TryParse(fields[3], out low) &&
                    decimal.TryParse(fields[4], out close) &&
                    decimal.TryParse(fields[5], out adjustedClose) &&
                    decimal.TryParse(fields[6], out volume))
                {
                    var stock = new StockData(symbol, date, open, high, low, close, adjustedClose, volume);
                    if (previous != null)
                    {
                        stock.PreviousClose = previous.Close;
                    }
                    stocks.Add(stock);
                    previous = stock;
                }

            }

            return stocks.OrderBy(x => x.Date);
        }

        private IEnumerable<StockData> GetWeeklyStocks(string symbol)
        {
            var dailyStocks = GetDailyStocks(symbol).ToList();
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
                while(i < dailyStocks.Count &&
                dailyStocks[i].Date.DayOfWeek >= endingDay.Date.DayOfWeek)
                {
                    endingDay = dailyStocks[i];
                    if(endingDay.Low < low)
                    {
                        low = endingDay.Low;
                    }
                    if(endingDay.High > high)
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

        public IEnumerable<string> GetSymbols()
        {
            DirectoryInfo d = new DirectoryInfo($"{_pathToStocks}");
            var files = d.GetFiles();
            return files.Select(x => x.Name);
        }
    }
}
