using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.DataAccess.Clients.TDAmeritrade
{
    public class PriceHistoryRequest
    {
        public string symbol;
        public string periodType = "ytd";
        public int period = 0;
        public string frequencyType;
        public int frequency = 1;
        public DateTime endDate;
        public DateTime startDate;

        public string GetRequestUrl()
        {
            var url = $"https://api.tdameritrade.com/v1/{symbol}/pricehistory?" +
                "periodType=" + periodType +
                "&period" + period +
                "&frequencyType";

            return url;
        }
    }
}
