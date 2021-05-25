using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class RSIFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _timeSampling = 5;
        private int _rsi = 14;

        public RSIFeatureExtractor(
            int timesampling = 5,
            int rsi = 14)
        {
            _timeSampling = timesampling;
            _rsi = rsi;
            Encoding = $"RSI{_rsi},{_timeSampling}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            double[] rsi;
            CalculateRSI(data, _rsi, out rsi);
            return rsi;
        }

        private void CalculateRSI(IEnumerable<(StockData Data, float Weight)> data,
            int rsiLength,
            out double[] rsi)
        {
            rsi = new double[_timeSampling];
            if (data.Count() < rsiLength + _timeSampling)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            for (int i = 0; i < rsi.Length; ++i)
            {
                var samples = ordered.Skip(i).Take(rsiLength);
                var avgGains = (double)samples.Average(x => x.Data.PercentChangeFromPreviousClose > 0.0m ? x.Data.PercentChangeFromPreviousClose : 0.0m);
                var avgLosses = (double)samples.Average(x => x.Data.PercentChangeFromPreviousClose <= 0.0m ? Math.Abs(x.Data.PercentChangeFromPreviousClose) : 0.0m);
                if (avgLosses < 0.01)
                {
                    rsi[i] = 1.0;
                }
                else
                {
                    rsi[i] = 1.0 - (1.0 / (1.0 + (avgGains / avgLosses)));
                }
            }
        }

    }
}
