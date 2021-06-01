using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class TTMSqueezeFeatureExtraction : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _timeSampling = 5;
        private int _sma = 20;

        public TTMSqueezeFeatureExtraction(
            int timesampling = 5,
            int sma = 20)
        {
            _timeSampling = timesampling;
            _sma = sma;
            Encoding = $"TTM{_sma},{_timeSampling}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            double[] ttmVals;
            CalculateTTMHistogram(data, _sma, out ttmVals);
            return ttmVals;
        }

        private void CalculateTTMHistogram(IEnumerable<(StockData Data, float Weight)> data,
            int smaLength,
            out double[] ttmHistogram)
        {

            ttmHistogram = new double[_timeSampling];
            if (data.Count() < smaLength)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();

            double max = 0.0;
            for (int i = 0; i < ttmHistogram.Length; ++i)
            {
                var samples = ordered.Skip(i).Take(smaLength);
                var high = (double)samples.Max(x => x.Data.High);
                var low = (double)samples.Min(x => x.Data.Low);
                var average = (double)samples.Average(x => x.Data.Average);
                var donchian = (high + low) / 2.0;


                ttmHistogram[i] = (double)ordered[i].Data.Close - ((donchian + average) / 2.0);
                if (Math.Abs(ttmHistogram[i]) > max)
                {
                    max = Math.Abs(ttmHistogram[i]);
                }
            }

            for (int i = 0; i < ttmHistogram.Length; ++i)
            {
                ttmHistogram[i] /= max;
            }
        }
    }
}
