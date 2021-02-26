using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.DataAccess.Clients.TDAmeritrade
{
    public class IntradayPriceHistoryRequest : IUrlProvider
    {
        public string ApiKey { get; set; }
        public string Symbol { get; set; }
        public int Frequency { get; set; } = 1;
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }

        public IntradayPriceHistoryRequest(string apiKey, string symbol)
        {
            Symbol = symbol;
            ApiKey = apiKey;
        }

        public string GetRequestUrl()
        {
            var url = $"https://api.tdameritrade.com/v1/marketdata/{Symbol}/pricehistory?apikey={ApiKey}&periodType=day&frequencyType=minute" +
                "&frequency=" + Frequency +
                "&endDate=" + new DateTimeOffset(EndDate).ToUnixTimeMilliseconds() +
                "&startDate=" + new DateTimeOffset(StartDate).ToUnixTimeMilliseconds();

            return url;
        }
    }
}
