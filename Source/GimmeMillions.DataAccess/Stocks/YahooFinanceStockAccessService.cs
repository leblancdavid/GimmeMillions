using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace GimmeMillions.DataAccess.Stocks
{
    public class YahooFinanceStockAccessService : IStockAccessService
    {
        private IStockRepository _stockRepository;
        private IStockHistoryRepository _stockHistoryRepository;
        private string _yahooHistoryBaseURL = "https://query1.finance.yahoo.com/v7/finance/download/";
        public YahooFinanceStockAccessService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
            _stockHistoryRepository = stockRepository.StockHistoryRepository;
        }

        public IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            return _stockRepository.GetStocks(symbol, period);
        }

        public IEnumerable<StockData> GetStocks(StockDataPeriod period, int limit = -1)
        {
            var symbols = GetSymbols();
            var stocks = new List<StockData>();
            foreach (var symbol in symbols)
            {
                stocks.AddRange(GetStocks(symbol, period));
            }
            return stocks;
        }

        public IEnumerable<string> GetSymbols()
        {
            return _stockRepository.GetSymbols();
        }


        public IEnumerable<StockData> UpdateStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            try
            {
                var lastUpdated = _stockHistoryRepository.GetLastUpdated(symbol);
              
                //F?period1=76204800&period2=1584316800&interval=1d&events=history
                WebClient webClient = new WebClient();
                DateTime startDate = new DateTime(2000, 1, 1);
                int period1 = (Int32)(startDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                int period2 = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string url = $"{_yahooHistoryBaseURL}{symbol}?period1={period1}&period2={period2}&interval=1d&events=history";

                string data = webClient.DownloadString(url);
                _stockHistoryRepository.AddOrUpdateStock(new StockHistory(symbol, data, StockDataPeriod.Day));

                //webClient.DownloadFile(url, $"{_pathToStocks}/{symbol}");
                //File.WriteAllText($"{_pathToStocks}/{symbol}", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving stock {symbol}: {ex.Message}");
                return _stockRepository.GetStocks(symbol, period);
            }

            return _stockRepository.GetStocks(symbol, period);
        }
    }
}