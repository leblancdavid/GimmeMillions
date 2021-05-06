using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class MACDHistogramFeatureExtraction : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _timeSampling = 5;
        private int _slowEma = 26;
        private int _fastEma = 12;
        private int _macdEma = 9;

        public MACDHistogramFeatureExtraction(
            int timesampling = 5,
            int slowEma = 26,
            int fastEma = 12,
            int macdEma = 9)
        {
            _timeSampling = timesampling;
            _slowEma = slowEma;
            _fastEma = fastEma;
            _macdEma = macdEma;
            Encoding = $"MACD{_slowEma},{_fastEma},{_macdEma},{_timeSampling}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            double[] macdVals;
            CalculateMACD(data, _slowEma, _fastEma, _macdEma, out macdVals);
            return macdVals;
        }

        private void CalculateMACD(IEnumerable<(StockData Data, float Weight)> data,
            int slowEmaLength, int fastEmaLength, int macdEmaLength,
            out double[] macdHistogram)
        {

            macdHistogram = new double[_timeSampling];
            if (data.Count() < slowEmaLength + macdEmaLength)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            var macdLine = new double[macdEmaLength + _timeSampling];
            double maxVal = 0.0001;
            for (int i = 0; i < macdLine.Length; ++i)
            {
                double slowEma = (double)ordered.Skip(i).Take(slowEmaLength).Average(x => x.Data.Close);
                double fastEma = (double)ordered.Skip(i).Take(fastEmaLength).Average(x => x.Data.Close);
                if (maxVal < slowEma)
                {
                    maxVal = slowEma;
                }
                macdLine[i] = (fastEma - slowEma);
            }

            double max = 0.0;
            for (int i = 0; i < macdHistogram.Length; ++i)
            {
                var signalLine = (double)macdLine.Skip(i).Take(_macdEma).Average();
                macdHistogram[i] = (macdLine[i] - signalLine) / maxVal;
                if(Math.Abs(macdHistogram[i]) > max)
                {
                    max = Math.Abs(macdHistogram[i]);
                }
            }

            for (int i = 0; i < macdHistogram.Length; ++i)
            {
                macdHistogram[i] /= max;
            }
        }
    }
}
