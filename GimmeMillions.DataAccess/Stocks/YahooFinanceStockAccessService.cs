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

        public IEnumerable<StockData> GetStocks(string symbol, FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily)
        {
            return _stockRepository.GetStocks(symbol, frequencyTimeframe);
        }

        public IEnumerable<StockData> GetStocks(FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily)
        {
            var symbols = GetSymbols();
            var stocks = new List<StockData>();
            foreach(var symbol in symbols)
            {
                stocks.AddRange(GetStocks(symbol, frequencyTimeframe));
            }
            return stocks;
        }

        public IEnumerable<StockData> GetStocks(string symbol, int timePeriod)
        {
            return _stockRepository.GetStocks(symbol, timePeriod);
        }

        public IEnumerable<string> GetSymbols()
        {
            return _stockRepository.GetSymbols();
        }

        private readonly string DOW_INDEX_SYMBOL = "^DJI";
        private readonly string SNP_INDEX_SYMBOL = "^GSPC";
        private readonly string NASDAQ_INDEX_SYMBOL = "^IXIC";
        private readonly string DOW_FUTURE_SYMBOL = "DIA";
        private readonly string SNP_FUTURE_SYMBOL = "SPY";
        private readonly string NASDAQ_FUTURE_SYMBOL = "QQQ";
        private readonly string RUSSEL_FUTURE_SYMBOL = "^RUT";

        public void UpdateFutures()
        {
            UpdateStocks(DOW_INDEX_SYMBOL);
            UpdateStocks(SNP_INDEX_SYMBOL);
            UpdateStocks(NASDAQ_INDEX_SYMBOL);
            UpdateStocks(DOW_FUTURE_SYMBOL);
            UpdateStocks(SNP_FUTURE_SYMBOL);
            UpdateStocks(NASDAQ_FUTURE_SYMBOL);
            UpdateStocks(RUSSEL_FUTURE_SYMBOL);
        }

        public IEnumerable<StockData> UpdateStocks(string symbol, FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily)
        {
            try
            {
                var lastUpdated = _stockHistoryRepository.GetLastUpdated(symbol);
                if(lastUpdated.Date == DateTime.Today)
                {
                    return _stockRepository.GetStocks(symbol, frequencyTimeframe);
                }
                //F?period1=76204800&period2=1584316800&interval=1d&events=history
                WebClient webClient = new WebClient();
                DateTime startDate = new DateTime(2000, 1, 1);
                int period1 = (Int32)(startDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                int period2 = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string url = $"{_yahooHistoryBaseURL}{symbol}?period1={period1}&period2={period2}&interval=1d&events=history";

                string data = webClient.DownloadString(url);
                _stockHistoryRepository.AddOrUpdateStock(new StockHistory(symbol, data));

                //webClient.DownloadFile(url, $"{_pathToStocks}/{symbol}");
                //File.WriteAllText($"{_pathToStocks}/{symbol}", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving stock {symbol}: {ex.Message}");
                return _stockRepository.GetStocks(symbol, frequencyTimeframe);
            }

            return _stockRepository.GetStocks(symbol, frequencyTimeframe);
        }
    }
}
