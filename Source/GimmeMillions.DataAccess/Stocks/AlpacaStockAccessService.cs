using Alpaca.Markets;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GimmeMillions.DataAccess.Stocks
{
    public class AlpacaStockAccessService : IStockAccessService
    {
        private string API_KEY = "PKZSK59ADESY8V9EC5SZ";
        private string API_SECRET = "93ia2Pfj9iyrgWf9BnZDJjzRqRAXshcKIbks0W4O";
        private IAlpacaDataClient _client;

        public AlpacaStockAccessService()
        {
            _client = Alpaca.Markets.Environments.Paper.GetAlpacaDataClient(new SecretKey(API_KEY, API_SECRET));
        }

        public IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period, int limit = 0)
        {
            var stocks = new List<StockData>();
            if (limit <= 0 || limit > 1000)
            {
                int runningCount = limit;
                if (limit <= 0)
                    runningCount = int.MaxValue;

                int count = 1000;
                var currentDate = DateTime.UtcNow;
                while (count == 1000 && runningCount > 0)
                {
                    var samples = GetStockDataFromClient(symbol, period, currentDate, runningCount > 1000 ? 1000 : runningCount);
                    if (!samples.Any())
                    {
                        break;
                    }
                    count = samples.Count();
                    runningCount -= count;
                    currentDate = samples.First().Date.AddMinutes(-1.0);
                    stocks.AddRange(samples.Reverse());
                }
                stocks.Reverse();
            }
            else
            {
                stocks.AddRange(GetStockDataFromClient(symbol, period, DateTime.UtcNow, limit));
            }

            for(int i = 1; i < stocks.Count; ++i)
            {
                stocks[i].PreviousClose = stocks[i - 1].Close;
            }

            return stocks;
        }

        private IEnumerable<StockData> GetStockDataFromClient(string symbol, StockDataPeriod period, DateTime time, int limit)
        {
            var request = new BarSetRequest(symbol, ToTimeFrame(period))
            {
                Limit = limit
            };

            Thread.Sleep(100);

            var bars = Task.Run(async () => await _client.GetBarSetAsync(TimeInterval.SetInclusiveTimeInterval(request, new DateTime(), time))).Result;

            var data = bars[symbol];
            var stocks = new List<StockData>();

            if (data.Count == 0)
                return stocks;


            foreach (var d in data)
            {
                stocks.Add(new StockData(symbol, d.TimeUtc.GetValueOrDefault(), d.Open, d.High, d.Low, d.Close, d.Close, d.Volume, d.Close));
            }

            return stocks;
        }

        private TimeFrame ToTimeFrame(StockDataPeriod period)
        {
            switch (period)
            {

                case StockDataPeriod.Hour:
                    return TimeFrame.Hour;
                case StockDataPeriod.FifteenMinute:
                    return TimeFrame.FifteenMinutes;
                case StockDataPeriod.FiveMinute:
                    return TimeFrame.FiveMinutes;
                case StockDataPeriod.Minute:
                    return TimeFrame.Minute;
                case StockDataPeriod.Week:
                case StockDataPeriod.Day:
                default:
                    return TimeFrame.Day;

            }
        }

        public IEnumerable<StockData> GetStocks(StockDataPeriod period, int limit = -1)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSymbols()
        {
            return new List<string>();
        }

        public IEnumerable<StockData> UpdateStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            return GetStocks(symbol, period, limit);
        }
    }
}