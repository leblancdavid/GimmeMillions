using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class KeltnerChannelFeatureExtraction : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _timeSampling = 5;
        private int _keltnerLength = 20;

        public KeltnerChannelFeatureExtraction(
            int timesampling = 5,
            int keltnerLength = 20)
        {
            _timeSampling = timesampling;
            _keltnerLength = keltnerLength;
            Encoding = $"Keltner{_keltnerLength},{_timeSampling}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            double[] keltner;
            CalculateKeltner(data, _keltnerLength, out keltner);
            return keltner;
        }

        private void CalculateKeltner(IEnumerable<(StockData Data, float Weight)> data,
            int keltnerLength,
            out double[] keltner)
        {

            keltner = new double[_timeSampling];
            if (data.Count() < keltnerLength + _timeSampling)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();

            double max = 0.0;
            for (int i = 0; i < _timeSampling; ++i)
            {
                var samples = ordered.Skip(i).Take(keltnerLength);
                var mean = samples.Average(x => x.Data.Average);
                var atr = samples.Average(x => x.Data.Range);

                keltner[i] = (double)((ordered[i].Data.Average - mean) / atr);
            }
        }
           
    }
}
