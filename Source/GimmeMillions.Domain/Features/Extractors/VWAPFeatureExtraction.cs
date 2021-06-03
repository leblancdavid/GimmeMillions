using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class VWAPFeatureExtraction : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _timeSampling = 5;
        private int _vwapLength = 10;

        public int OutputLength { get; private set; }

        public VWAPFeatureExtraction(
            int timesampling = 5,
            int vwapLength = 10)
        {
            _timeSampling = timesampling;
            OutputLength = _timeSampling;
            _vwapLength = vwapLength;
            Encoding = $"VWAP{_vwapLength},{_timeSampling}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            double[] vwap;
            CalculateVWAP(data, _vwapLength, out vwap);
            return vwap;
        }

        private void CalculateVWAP(IEnumerable<(StockData Data, float Weight)> data,
            int vwapLength,
            out double[] vwapHistogram)
        {
            vwapHistogram = new double[_timeSampling];
            if (data.Count() < vwapLength + _timeSampling)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            double maxVal = 0.0001;
            for (int i = 0; i < vwapHistogram.Length; ++i)
            {
                var samples = ordered.Skip(i).Take(vwapLength);
                var totalVolume = samples.Sum(x => x.Data.Volume);
                if (totalVolume < 0.0001m)
                {
                    vwapHistogram[i] = (double)ordered[i].Data.Average;
                }
                else
                {
                    var vwapAvg = (double)samples.Sum(x => x.Data.Average * x.Data.Volume) / (double)totalVolume;
                    vwapHistogram[i] = ((double)ordered[i].Data.Average - vwapAvg);
                }
                if (Math.Abs(vwapHistogram[i]) > maxVal)
                {
                    maxVal = Math.Abs(vwapHistogram[i]);
                }
            }

            for (int i = 0; i < vwapHistogram.Length; ++i)
            {
                vwapHistogram[i] /= maxVal;
            }
           
        }
    }
}
