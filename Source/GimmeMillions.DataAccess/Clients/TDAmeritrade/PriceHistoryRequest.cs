using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.DataAccess.Clients.TDAmeritrade
{
    public class PriceHistoryRequest
    {
        public string Symbol { get; set; }
        public string FrequencyType { get; set; }
        public int Frequency { get; set; } = 1;
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }

        public PriceHistoryRequest(string symbol)
        {
            Symbol = symbol;
        }

        public string GetRequestUrl()
        {
            var url = $"https://api.tdameritrade.com/v1/marketdata/{Symbol}/pricehistory?periodType=year" +
                "&frequencyType=" + FrequencyType +
                "&frequency=" + Frequency +
                "&endDate=" + new DateTimeOffset(EndDate).ToUnixTimeMilliseconds() +
                "&startDate=" + new DateTimeOffset(StartDate).ToUnixTimeMilliseconds();

            return url;
        }
    }
}
