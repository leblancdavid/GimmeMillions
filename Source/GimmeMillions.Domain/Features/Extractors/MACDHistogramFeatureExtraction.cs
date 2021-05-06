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
            Encoding = $"MACD({_slowEma},{_fastEma},{_macdEma}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var featureVector = new List<double>();

            CalculateMACD(data, _slowEma, _fastEma, _macdEma, out macdVals);
            return macdVals;

        }

        private void CalculateMACD(IEnumerable<(StockData Data, float Weight)> data,
            int slowEmaLength, int fastEmaLength, int macdEmaLength,
            out double[] macd)
        {

            macd = new double[_timeSampling];
            if (data.Count() < slowEmaLength + macdEmaLength)
            {
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            var macdLine = new double[macdEmaLength + macdSlopeLength];
            double maxVal = 0.0001;
            for (int i = 0; i < macdLine.Length; ++i)
            {
                double slowEma = (double)ordered.Skip(i).Take(slowEmaLength).Average(x => x.Data.Close);
                double fastEma = (double)ordered.Skip(i).Take(fastEmaLength).Average(x => x.Data.Close);
                if(maxVal < slowEma)
                {
                    maxVal = slowEma;
                }
                macdLine[i] = (fastEma - slowEma);
            }

            var macdHistogram = new double[macdSlopeLength];
            for (int i = 0; i < macdHistogram.Length; ++i)
            {
                var signalLine = (double)macdLine.Skip(i).Take(_macdEma).Average();
                macdHistogram[i] = (macdLine[i] - signalLine) / maxVal;
            }
            macd = macdHistogram[0];
            if(double.IsNaN(macd))
            {
                macd = 0.0;
            }
            var kernel = GetNormalizedWavelet(macdSlopeLength);
            macdSlope = 0.0;
            for(int i = 0; i < macdSlopeLength; ++i)
            {
                macdSlope += macdHistogram[i] * kernel[i];
            }
        }

        private void CalculateVWAP(IEnumerable<(StockData Data, float Weight)> data,
            int vwapLength, int vwapSlopLength,
            out double vwap, out double vwapSlope)
        {
            if(data.Count() < vwapLength + vwapSlopLength)
            {
                vwap = 0.0;
                vwapSlope = 0.0;
                return;
            }

            var vwapHistogram = new double[vwapSlopLength];
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            double maxVal = 0.0001;
            for (int i = 0; i < vwapHistogram.Length; ++i)
            {
                var samples = ordered.Skip(i).Take(vwapLength);
                var totalVolume = samples.Sum(x => x.Data.Volume);
                if(totalVolume < 0.0001m)
                {
                    vwapHistogram[i] = (double)ordered[i].Data.Close;
                }
                else
                {
                    var vwapAvg = (double)samples.Sum(x => x.Data.Close * x.Data.Volume) / (double)totalVolume;
                    vwapHistogram[i] = ((double)ordered[i].Data.Close - vwapAvg);
                }
                if (Math.Abs(vwapHistogram[i]) > maxVal)
                {
                    maxVal = Math.Abs(vwapHistogram[i]);
                }
            }

            vwap = vwapHistogram[0] / maxVal;
            if (double.IsNaN(vwap))
            {
                vwap = 0.0;
            }
            //vwapSlope = vwapHistogram.Skip(1).Average(x => vwapHistogram[0] - x);
            var kernel = GetNormalizedWavelet(vwapSlopLength);
            vwapSlope = 0.0;
            for (int i = 0; i < vwapSlopLength; ++i)
            {
                vwapSlope += vwapHistogram[i] * kernel[i] / maxVal;
            }
        }
        
        private void CalculateRSI(IEnumerable<(StockData Data, float Weight)> data,
            int rsiLength, int rsiSlopeLength,
            out double rsi, out double rsiSlope)
        {
            if (data.Count() < rsiLength + rsiSlopeLength)
            {
                rsi = 0.0;
                rsiSlope = 0.0;
                return;
            }

            var rsiHistogram = new double[rsiSlopeLength];
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            for (int i = 0; i < rsiHistogram.Length; ++i)
            {
                var samples = ordered.Skip(i).Take(rsiLength);
                var avgGains = (double)samples.Average(x => x.Data.PercentChangeFromPreviousClose > 0.0m ? x.Data.PercentChangeFromPreviousClose : 0.0m);
                var avgLosses = (double)samples.Average(x => x.Data.PercentChangeFromPreviousClose <= 0.0m ? Math.Abs(x.Data.PercentChangeFromPreviousClose) : 0.0m);
                if(avgLosses < 0.01)
                {
                    rsiHistogram[i] = 1.0;
                }
                else
                {
                    rsiHistogram[i] = 1.0 - (1.0 / (1.0 + (avgGains / avgLosses)));
                }
            }

            rsi = rsiHistogram[0];
            if (double.IsNaN(rsi))
            {
                rsi = 0.0;
            }
            //rsiSlope = rsiHistogram.Skip(1).Average(x => rsiHistogram[0] - x);
            var kernel = GetNormalizedWavelet(rsiSlopeLength);
            rsiSlope = 0.0;
            for (int i = 0; i < rsiSlopeLength; ++i)
            {
                rsiSlope += rsiHistogram[i] * kernel[i];
            }
        }

        private void CalculateCMF(IEnumerable<(StockData Data, float Weight)> data,
            int cmfLength, int cmfSlopeLength,
            out double cmf, out double cmfSlope)
        {
            if (data.Count() < cmfLength + cmfSlopeLength)
            {
                cmf = 0.0;
                cmfSlope = 0.0;
                return;
            }

            var cmfHistogram = new double[cmfSlopeLength];
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            double maxVal = 0.0001;
            for (int i = 0; i < cmfHistogram.Length; ++i)
            {
                var samples = ordered.Skip(i).Take(cmfLength);
                var totalVolume = samples.Sum(x => x.Data.Volume);
                if(totalVolume < 0.0001m)
                {
                    cmfHistogram[i] = 0;
                }
                else
                {
                    cmfHistogram[i] = (double)(samples.Sum(x => x.Data.CMF * x.Data.Volume) / totalVolume);
                }
                if(Math.Abs(cmfHistogram[i]) > maxVal)
                {
                    maxVal = Math.Abs(cmfHistogram[i]);
                }
            }

            cmf = cmfHistogram[0] / maxVal;
            if (double.IsNaN(cmf))
            {
                cmf = 0.0;
            }
            //cmfSlope = cmfHistogram.Skip(1).Average(x => cmfHistogram[0] - x);
            var kernel = GetNormalizedWavelet(cmfSlopeLength);
            cmfSlope = 0.0;
            for (int i = 0; i < cmfSlopeLength; ++i)
            {
                cmfSlope += cmfHistogram[i] * kernel[i] / maxVal;
            }
        }

        private double[] Normalize(double[] input)
        {
            double max = input.Max(x => Math.Abs(x));
            var output = new double[input.Length];
            if (max < 0.0001)
                return output;
            for(int i = 0; i < input.Length; ++i)
            {
                output[i] = input[i] / max;
            }
            return output;
        }

        private double[] GetNormalizedWavelet(int length)
        {
            double sigma = (double)length / 6.0;
            double e = 2.71828;
            double pi = 3.14159;
            double f = 2.0 / (Math.Sqrt(3.0 * sigma) * Math.Pow(pi, 0.25));
            int center = length / 2;
            var kernel = new double[length];
            for(int i = 0; i < length; ++i)
            {
                double t = i - center;
                kernel[i] = f * (1.0 - Math.Pow(t / sigma, 2.0)) *
                    Math.Pow(e, -1.0 * t * t / (2.0 * sigma * sigma));
            }

            return kernel;
        }
    }
}
