using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class FibonacciRetracement
    {
        public FibonacciRetracement(decimal high, decimal low)
        {
            decimal diff = high - low;
            _fibonacciPrices = new decimal[_fibonacciRatios.Length];
            for(int i = 0; i < _fibonacciRatios.Length; ++i)
            {
                _fibonacciPrices[i] = diff * _fibonacciRatios[i] + low;
            }
            High = high;
            Low = low;
        }

        public decimal High { get; private set; }
        public decimal Low { get; private set; }

        private decimal[] _fibonacciPrices;
        public decimal[] FibonacciPrices
        {
            get
            {
                return _fibonacciPrices;
            }
        }

        private decimal[] _fibonacciRatios = new decimal[]
        {
            0.0m, 0.236m, 0.382m, 0.5m, 0.618m, 1.0m, 1.618m, 2.618m
        };
        public decimal[] FibonacciRatios
        {
            get
            {
                return _fibonacciRatios;
            }
        }

        public int NearestFibonacci(StockData data, out decimal distance)
        {
            distance = decimal.MaxValue;
            int minIndex = -1;
            int i = 0;
            foreach(var price in _fibonacciPrices)
            {
                decimal diff = Math.Abs(data.Open - price);
                if (diff < distance)
                {
                    distance = diff;
                    minIndex = i;
                }
                diff = Math.Abs(data.Close - price);
                if (diff < distance)
                {
                    distance = diff;
                    minIndex = i;
                }
                diff = Math.Abs(data.High - price);
                if (diff < distance)
                {
                    distance = diff;
                    minIndex = i;
                }
                diff = Math.Abs(data.Low - price);
                if (diff < distance)
                {
                    distance = diff;
                    minIndex = i;
                }
                i++;
            }

            return minIndex;
        }

        public decimal GetFibonacciValue(StockData data)
        {
            return (data.Close - Low) / (High - Low);
        }
    }
}
