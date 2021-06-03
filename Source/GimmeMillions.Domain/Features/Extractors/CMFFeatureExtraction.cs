using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class CMFFeatureExtraction : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _timeSampling = 5;
        private int _cmf = 21;
        public int OutputLength { get; private set; }

        public CMFFeatureExtraction(
            int timesampling = 5,
            int cmf = 21)
        {
            _timeSampling = timesampling;
            OutputLength = _timeSampling;
            _cmf = cmf;
            Encoding = $"CMF{_cmf},{_timeSampling}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            double[] cmfVals;
            CalculateCMF(data, _cmf, out cmfVals);
            return cmfVals;
        }

        private void CalculateCMF(IEnumerable<(StockData Data, float Weight)> data,
            int cmfLength,
            out double[] cmfHistogram)
        {

            cmfHistogram = new double[_timeSampling];
            if (data.Count() < _timeSampling + cmfLength)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            double maxVal = 0.0001;
            for (int i = 0; i < cmfHistogram.Length; ++i)
            {
                var samples = ordered.Skip(i).Take(cmfLength);
                var totalVolume = samples.Sum(x => x.Data.Volume);
                if (totalVolume < 0.0001m)
                {
                    cmfHistogram[i] = 0;
                }
                else
                {
                    cmfHistogram[i] = (double)(samples.Sum(x => x.Data.CMF * x.Data.Volume) / totalVolume);
                }
                if (Math.Abs(cmfHistogram[i]) > maxVal)
                {
                    maxVal = Math.Abs(cmfHistogram[i]);
                }
            }
            for (int i = 0; i < cmfHistogram.Length; ++i)
            {
                cmfHistogram[i] /= maxVal;
            }
        }
    }
}
