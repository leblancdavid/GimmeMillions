﻿using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks.Filters
{
    public class DefaultStockFilter : IStockFilter
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private decimal _minPrice;
        private decimal _maxPrice;
        private decimal _minVolume;
        private decimal _maxVolume;
        private decimal _maxPercentHigh;
        private decimal _maxPercentLow;
        public DefaultStockFilter(DateTime start = default(DateTime),
            DateTime end = default(DateTime),
            decimal minPrice = decimal.MinValue, decimal maxPrice = decimal.MaxValue,
            decimal minVolume = decimal.MinValue, decimal maxVolume = decimal.MaxValue,
            decimal maxPercentHigh = decimal.MaxValue,
            decimal maxPercentLow = decimal.MaxValue)
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
            _maxPercentHigh = maxPercentHigh;
            _maxPercentLow = maxPercentLow;
        }

        public bool Pass(StockData stockData)
        {
            if (stockData.Date >= _startTime && stockData.Date <= _endTime &&
                stockData.Volume >= _minVolume && stockData.Volume <= _maxVolume &&
                stockData.Open >= _minPrice && stockData.Open <= _maxPrice &&
                stockData.PercentChangeHighToPreviousClose < _maxPercentHigh &&
                Math.Abs(stockData.PercentChangeLowToPreviousClose) < _maxPercentLow)
                return true;
            return false;
        }
    }
}
