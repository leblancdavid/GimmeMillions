using GimmeMillions.DataAccess.Clients.TDAmeritrade;
using GimmeMillions.Domain.Stocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GimmeMillions.DataAccess.Stocks
{
    public class TDAmeritradeStockAccessService : IStockAccessService
    {
        private readonly TDAmeritradeApiClient _client;
        private readonly IStockSymbolsRepository _stockSymbolsRepository;
        public TDAmeritradeStockAccessService(TDAmeritradeApiClient client, IStockSymbolsRepository stockSymbolsRepository)
        {
            _client = client;
            _stockSymbolsRepository = stockSymbolsRepository;
        }

        public IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            var response = _client.GetPriceHistory(BuildPriceHistoryRequest(symbol, period, limit));
            if(response.IsSuccessStatusCode)
            {
                return ParseResponseContent(response.Content.ReadAsStringAsync().Result, symbol);
            }
            return new List<StockData>();
        }

        private IEnumerable<StockData> ParseResponseContent(string content, string symbol)
        {
            var stocks = new List<StockData>();
            try
            {
                var candles = JsonConvert.DeserializeObject<JObject>(content)["candles"] as JArray; 
               
                var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                decimal previousClose = 0.0m;
                foreach (var sample in candles)
                {
                    var date = origin.AddMilliseconds((long)sample["datetime"]);
                    stocks.Add(new StockData(symbol, date,
                        (decimal)sample["open"], (decimal)sample["high"], (decimal)sample["low"], (decimal)sample["close"], 
                        (decimal)sample["close"], (decimal)sample["volume"], previousClose));
                    previousClose = (decimal)sample["close"];
                }
            }
            catch(Exception)
            {

            }
            return stocks;
        }
        private PriceHistoryRequest BuildPriceHistoryRequest(string symbol, StockDataPeriod period, int limit)
        {
            string freqType = "day";
            int freq = 1;
            bool intraday = false;
            switch (period)
            {
                case StockDataPeriod.Week:
                    freqType = "weekly";
                    break;
                case StockDataPeriod.Day:
                    freqType = "daily";
                    break;
                case StockDataPeriod.SixHour:
                    freqType = "daily";//not supported, default to daily
                    break;
                case StockDataPeriod.Hour:
                    freqType = "daily";//not supported, default to daily
                    break;
                case StockDataPeriod.FifteenMinute:
                    freqType = "minute";
                    freq = 15;
                    intraday = false;
                    break;
                case StockDataPeriod.FiveMinute:
                    freqType = "minute";
                    freq = 5;
                    intraday = false;
                    break;
                case StockDataPeriod.Minute:
                    freqType = "minute";
                    freq = 1;
                    intraday = false;
                    break;
            }

            var endDate = DateTime.UtcNow;
            var startDate = new DateTime();
            if (limit > 0)
            {
                if (intraday)
                {
                    startDate = endDate.AddMinutes(-1 * limit * freq);
                }
                else
                {
                    startDate = endDate.AddDays(-1 * limit);
                }
            }

            return new PriceHistoryRequest(_client.ApiKey, symbol)
            {
                EndDate = endDate,
                StartDate = startDate,
                Frequency = freq,
                FrequencyType = freqType
            };
        }

        public IEnumerable<string> GetSymbols()
        {
            return _stockSymbolsRepository.GetStockSymbols();
        }

        public IEnumerable<StockData> UpdateStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            return GetStocks(symbol, period, limit);
        }
    }
}
