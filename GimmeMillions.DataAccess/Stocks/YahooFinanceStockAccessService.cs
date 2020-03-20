﻿using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Net;

namespace GimmeMillions.DataAccess.Stocks
{
    public class YahooFinanceStockAccessService : IStockAccessService
    {
        private IStockRepository _stockRepository;
        private string _yahooHistoryBaseURL = "https://query1.finance.yahoo.com/v7/finance/download/";
        private string _pathToStocks;
        public YahooFinanceStockAccessService(IStockRepository stockRepository, string pathToStocks)
        {
            _stockRepository = stockRepository;
            _pathToStocks = pathToStocks;
        }

        public IEnumerable<StockData> GetStocks(string symbol)
        {
            return _stockRepository.GetStocks(symbol);
        }

        public IEnumerable<StockData> UpdateStocks(string symbol)
        {
            try
            {
                //F?period1=76204800&period2=1584316800&interval=1d&events=history
                WebClient webClient = new WebClient();
                DateTime startDate = new DateTime(2000, 1, 1);
                int period1 = (Int32)(startDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                int period2 = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string url = $"{_yahooHistoryBaseURL}{symbol}?period1={period1}&period2={period2}&interval=1d&events=history";
                webClient.DownloadFile(url, $"{_pathToStocks}/{symbol}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving stock {symbol}: {ex.Message}");
            }

            return _stockRepository.GetStocks(symbol);
        }
    }
}
