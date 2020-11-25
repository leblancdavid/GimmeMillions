using GimmeMillions.Domain.Stocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace GimmeMillions.DataAccess.Stocks
{
    public class CoinbaseApiAccessService : IStockAccessService
    {
        private string _secret;
        private string _key;
        private string _passphrase;
        private HMACSHA256 _hmac;
        private HttpClient _client = new HttpClient();
        public CoinbaseApiAccessService(string secret, string key, string passphrase)
        {
            _secret = secret;
            _key = key;
            _passphrase = passphrase;

            _client.BaseAddress = new Uri("https://api.pro.coinbase.com/");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _key);
            _client.DefaultRequestHeaders.Add("CB-ACCESS-PASSPHRASE", _passphrase);
            _client.DefaultRequestHeaders.Add("User-Agent", "CoinbaseApiAccessService");

            _hmac = new HMACSHA256(System.Convert.FromBase64String(_secret));

        }

        public IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            int days = limit;
            if(days <= 0)
            {
                days = 30; //default to 30 days worth of data
            }

            var end = DateTime.UtcNow;
            var start = end.AddDays(-1.0); 
            var stocks = new List<StockData>();
            for (int i = 0; i < days; ++i)
            {
                stocks.AddRange(GetStocks(symbol, period, start, end));
                end = start;
                start = end.AddDays(-1.0);
            }

            if (!stocks.Any())
                return stocks;
            stocks = stocks.OrderBy(x => x.Date).ToList();
            decimal previousClose = stocks.First().Close;
            foreach(var stock in stocks)
            {
                stock.PreviousClose = previousClose;
                previousClose = stock.Close;
            }

            return stocks;
        }

        private IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period, DateTime start, DateTime end)
        {
            try
            {
                string requestUrl = $"/products/{symbol}/candles?start={start.ToString("o")}&end={end.ToString("o")}&granularity={(int)period}";
                string timestamp, signing;
                GetRequestSigning(requestUrl, "GET", out signing, out timestamp);
                _client.DefaultRequestHeaders.Remove("CB-ACCESS-SIGN");
                _client.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signing);
                _client.DefaultRequestHeaders.Remove("CB-ACCESS-TIMESTAMP");
                _client.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);

                //need to throttle the requests by 1 sec
                Thread.Sleep(1000);
                var response = _client.GetAsync(requestUrl).Result;
                if(response.IsSuccessStatusCode)
                    return ParseResponseContent(response.Content.ReadAsStringAsync().Result, symbol, period);  //Make sure to add a reference to System.Net.Http.Formatting.dll

            }
            catch (Exception ex)
            {
                
            }
            return new List<StockData>();
        }

        private IEnumerable<StockData> ParseResponseContent(string content, string symbol, StockDataPeriod period)
        {
            var stocks = new List<StockData>();
            var contentArray = JsonConvert.DeserializeObject<JArray[]>(content);
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            foreach (var sample in contentArray)
            {
                var date = origin.AddSeconds((long)sample[0]);
                stocks.Add(new StockData(symbol, date,
                    (decimal)(double)sample[3], (decimal)(double)sample[1],
                    (decimal)(double)sample[2], (decimal)(double)sample[4],
                    (decimal)(double)sample[4], (decimal)(double)sample[5],
                    (decimal)(double)sample[4]));
            }
            return stocks;
        }

        public IEnumerable<string> GetSymbols()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockData> UpdateStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            return GetStocks(symbol, period, limit);
        }

        private void GetRequestSigning(string path, string method, out string sign, out string timestamp)
        {
            timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string what = timestamp + method + path;
            sign = Convert.ToBase64String(_hmac.ComputeHash(Encoding.UTF8.GetBytes(what)));

        }
    }
}
