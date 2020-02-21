using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.IO;

namespace GimmeMillions.DataAccess.Stocks
{
    public class StockDataRepository : IStockRepository
    {
        private readonly string _pathToStocks;
        public StockDataRepository(string pathToStocks)
        {
            _pathToStocks = pathToStocks;
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
    }
}
