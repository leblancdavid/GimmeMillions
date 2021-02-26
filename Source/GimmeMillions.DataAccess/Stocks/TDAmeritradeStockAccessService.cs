using GimmeMillions.DataAccess.Clients.TDAmeritrade;
using GimmeMillions.Domain.Stocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            if(period >= StockDataPeriod.Day)
            {
                var response = _client.GetPriceHistory(BuildPriceHistoryRequest(symbol, period, limit));
                if (response.IsSuccessStatusCode)
                {
                    return ParseResponseContent(response.Content.ReadAsStringAsync().Result, symbol);
                }
            }
            else
            {
                var stockData = new List<StockData>();
                bool done = false;
                DateTime endDate = DateTime.UtcNow, startDate;
                do
                {
                    startDate = endDate.AddDays(-1.0);
                    var response = _client.GetPriceHistory(new IntradayPriceHistoryRequest(_client.ApiKey, symbol)
                    {
                        EndDate = endDate,
                        StartDate = startDate,
                        Frequency = GetFrequency(period)
                    });
                    if (!response.IsSuccessStatusCode)
                    {
                        done = true;
                        //return ParseResponseContent(response.Content.ReadAsStringAsync().Result, symbol);
                    }
                    else
                    {
                        var segment = ParseResponseContent(response.Content.ReadAsStringAsync().Result, symbol).Where(x => x.Date >= startDate && x.Date < endDate).ToList();
                        if (!segment.Any())
                            done = true;
                        else
                        {
                            if(stockData.Any())
                                stockData[0].PreviousClose = segment.Last().Close;

                            stockData.InsertRange(0, segment);

                            endDate = stockData.First().Date;
                            if (stockData.Count >= limit)
                                done = true;
                        }
                    }


                } while (!done);
                return stockData;
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
                    intraday = true;
                    break;
                case StockDataPeriod.FiveMinute:
                    freqType = "minute";
                    freq = 5;
                    intraday = true;
                    break;
                case StockDataPeriod.Minute:
                    freqType = "minute";
                    freq = 1;
                    intraday = true;
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
                Frequency = GetFrequency(period),
                FrequencyType = freqType
            };
        }

        private int GetFrequency(StockDataPeriod period)
        {
            switch (period)
            {
                case StockDataPeriod.Week:
                    return 5;
                case StockDataPeriod.FifteenMinute:
                    return 15;
                case StockDataPeriod.FiveMinute:
                    return 5;
                case StockDataPeriod.SixHour:
                    return 6;
                case StockDataPeriod.Minute:
                case StockDataPeriod.Day:
                case StockDataPeriod.Hour:
                default:
                    return 1;
            }
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
