using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace GimmeMillions.DataAccess.Stocks
{
    public class CoinbaseApiAccessService : IStockAccessService
    {
        private string _secret = "Fafav1z7cSnlulnPwdAl2pv1B6CMkM6b4If0WenT8hdgR9+ZlsowruJBxiUJv9SMmHvKWy6X4OQdBN2YaZGdyQ==";
        private string _key = "059dd44fa67976e9743836fd0a3a5624";
        private string _passphrase = "23ferghfa21abb";
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

            _hmac = new HMACSHA256(System.Convert.FromBase64String(_secret));

        }

        //For now keys are hardcoded
        public CoinbaseApiAccessService()
        {
            _client.BaseAddress = new Uri("https://api.pro.coinbase.com/");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _key);
            _client.DefaultRequestHeaders.Add("CB-ACCESS-PASSPHRASE", _passphrase); 
            _client.DefaultRequestHeaders.Add("User-Agent", "CoinbaseApiAccessService");

            _hmac = new HMACSHA256(System.Convert.FromBase64String(_secret));
        }

        public IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period, int limit = -1)
        {
            var stocks = new List<StockData>();
            try
            {
                var end = DateTime.UtcNow;
                var start = end.AddDays(-30.0);
                if (limit > 0)
                {
                    start = end.AddDays(-1.0 * limit);
                }

                string requestUrl = $"/products/{symbol}/candles?start={start.ToString("o")}&end={end.ToString("o")}&granularity={(int)period}";
                string timestamp, signing;
                GetRequestSigning(requestUrl, "GET", out signing, out timestamp);
                _client.DefaultRequestHeaders.Remove("CB-ACCESS-SIGN");
                _client.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signing);
                _client.DefaultRequestHeaders.Remove("CB-ACCESS-TIMESTAMP");
                _client.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);

                var response = _client.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
               
            }
            catch(Exception ex)
            {

            }

            return stocks;
        }

        public IEnumerable<StockData> GetStocks(StockDataPeriod period, int limit = -1)
        {
            throw new NotImplementedException();
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
