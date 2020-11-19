using System;

namespace GimmeMillions.Domain.Stocks.Filters
{
    public class MarketHoursUtcFilter : IStockFilter
    {
        private TimeSpan UTC_OPEN = new TimeSpan(14, 30, 0);
        private TimeSpan UTC_CLOSE = new TimeSpan(21, 0, 0);
        public bool Pass(StockData stockData)
        {
            if (stockData.Date.TimeOfDay < UTC_OPEN || stockData.Date.TimeOfDay > UTC_CLOSE)
            {
                return false;
            }

            return true;
        }
    }
}
