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
                var stock = new StockData(symbol,
                    DateTime.Parse(fields[0]),
                    decimal.Parse(fields[1]),
                    decimal.Parse(fields[2]),
                    decimal.Parse(fields[3]),
                    decimal.Parse(fields[4]),
                    decimal.Parse(fields[5]));
                if(previous != null)
                {
                    stock.PercentChange = (decimal)100.0 * (stock.Close - previous.Close) / previous.Close;
                }
                stocks.Add(stock);
                previous = stock;
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
