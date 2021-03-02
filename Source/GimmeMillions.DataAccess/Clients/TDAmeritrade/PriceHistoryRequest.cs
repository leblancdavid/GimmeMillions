using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.DataAccess.Clients.TDAmeritrade
{
    public class PriceHistoryRequest : IUrlProvider
    {
        public string ApiKey { get; set; }
        public string Symbol { get; set; }
        public string FrequencyType { get; set; }
        public int Frequency { get; set; } = 1;
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }

        public PriceHistoryRequest(string apiKey, string symbol)
        {
            Symbol = symbol;
            ApiKey = apiKey;
        }

        public string GetRequestUrl(bool authenticated = false)
        {
            string url = "";
            if (authenticated)
            {
                url = $"https://api.tdameritrade.com/v1/marketdata/{Symbol}/pricehistory?periodType=year";
            }
            else
            {
                url = $"https://api.tdameritrade.com/v1/marketdata/{Symbol}/pricehistory?apikey={ApiKey}&periodType=year";
            }

            url += "&frequencyType=" + FrequencyType +
                "&frequency=" + Frequency +
                "&endDate=" + new DateTimeOffset(EndDate).ToUnixTimeMilliseconds() +
                "&startDate=" + new DateTimeOffset(StartDate).ToUnixTimeMilliseconds();
            return url;
        }
    }
}
