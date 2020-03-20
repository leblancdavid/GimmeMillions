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

        public Result<StockData> GetStock(string symbol, DateTime date)
        {
            var stockFound = GetStocks(symbol, date, date);
            if(!stockFound.Any())
            {
                return Result.Failure<StockData>($"Unable to retrieve stock {symbol} for date {date.ToString("MM/dd/yyyy")}");
            }
            return Result.Ok(stockFound.First());
        }

        public IEnumerable<StockData> GetStocks(string symbol)
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
                decimal open, high, low, close, adjustedClose;
                if(DateTime.TryParse(fields[0], out date) &&
                    decimal.TryParse(fields[1], out open) &&
                    decimal.TryParse(fields[2], out high) &&
                    decimal.TryParse(fields[3], out low) &&
                    decimal.TryParse(fields[4], out close) &&
                    decimal.TryParse(fields[5], out adjustedClose))
                {
                    var stock = new StockData(symbol, date, open, high, low, close, adjustedClose);
                    if (previous != null)
                    {
                        stock.PercentChange = (decimal)100.0 * (stock.Close - previous.Close) / previous.Close;
                    }
                    stocks.Add(stock);
                    previous = stock;
                }
                
            }

            return stocks;
        }

        public IEnumerable<StockData> GetStocks(string symbol, DateTime start, DateTime end)
        {
            return GetStocks(symbol).Where(x => x.Date >= start && x.Date <= end);
        }

        public IEnumerable<string> GetSymbols()
        {
            return Directory.GetFiles($"{_pathToStocks}");
        }
    }
}
