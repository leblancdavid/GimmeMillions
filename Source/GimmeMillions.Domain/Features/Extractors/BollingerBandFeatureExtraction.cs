using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class BollingerBandFeatureExtraction : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _timeSampling = 5;
        private int _bollingerLength = 20;
        private int _fastEma = 12;
        private int _macdEma = 9;

        public BollingerBandFeatureExtraction(
            int timesampling = 5,
            int bollingerLength = 20)
        {
            _timeSampling = timesampling;
            _bollingerLength = bollingerLength;
            Encoding = $"Bollinger{_bollingerLength},{_timeSampling}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            double[] bollinger;
            CalculateBollinger(data, _bollingerLength, out bollinger);
            return bollinger;
        }

        private void CalculateBollinger(IEnumerable<(StockData Data, float Weight)> data,
            int bollingerLength,
            out double[] bollinger)
        {

            bollinger = new double[_timeSampling];
            if (data.Count() < bollingerLength + _timeSampling)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();

            double max = 0.0;
            for (int i = 0; i < _timeSampling; ++i)
            {
                var mean = ordered.Skip(i).Take(bollingerLength).Average(x => x.Data.Average);
                var stdev = Math.Sqrt(ordered.Sum(x => Math.Pow((double)(x.Data.Average - mean), 2.0)) / (double)bollingerLength);

                bollinger[i] = (double)(ordered[i].Data.Average - mean) / stdev;
            }
        }
           
    }
}
