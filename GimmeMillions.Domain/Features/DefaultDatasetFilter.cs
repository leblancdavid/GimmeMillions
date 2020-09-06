using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class DefaultDatasetFilter : IDatasetFilter
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private decimal _minPrice;
        private decimal _maxPrice;
        private decimal _minVolume;
        private decimal _maxVolume;
        public DefaultDatasetFilter(DateTime start = default(DateTime),
            DateTime end = default(DateTime),
            decimal minPrice = 0.0m, decimal maxPrice = decimal.MaxValue,
            decimal minVolume = 0.0m, decimal maxVolume = decimal.MaxValue)
        {
            _startTime = start;
            if (end == default(DateTime))
                _endTime = DateTime.MaxValue;
            else
                _endTime = end;
            _minPrice = minPrice;
            _maxPrice = maxPrice;
            _minVolume = minVolume;
            _maxVolume = maxVolume;
        }

        public bool Pass(StockData stockData)
        {
            if (stockData.Date >= _startTime && stockData.Date <= _endTime &&
                stockData.Volume >= _minVolume && stockData.Volume >= _maxVolume &&
                stockData.Open >= _minPrice && stockData.Open <= _maxPrice)
                return true;
            return false;
        }
    }
}
