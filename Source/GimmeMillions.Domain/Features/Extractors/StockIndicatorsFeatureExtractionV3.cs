using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class StockIndicatorsFeatureExtractionV3 : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _bollingerLength = 40;
        private int _timeSampling = 4;
        private int _slowEma = 32;
        private int _fastEma = 16;
        private int _macdEma = 12;
        private int _macdSlope = 7;
        private int _vwap = 12;
        private int _vwapSlope = 7;
        private int _rsi = 12;
        private int _rsiSlope = 7;
        private int _cmf = 24;
        private int _cmfSlope = 7;
        private int _version = 2;
        private bool _normalize = false;

        public StockIndicatorsFeatureExtractionV3(
            int timesampling = 4,
            int bollingerLength = 40,
            int slowEma = 32,
            int fastEma = 16,
            int macdEma = 12,
            int macdSlope = 7,
            int vwap = 12,
            int vwapSlope = 7,
            int rsi = 12,
            int rsiSlope = 7,
            int cmf = 24,
            int cmfSlope = 7,
            bool normalize = false)
        {
            _timeSampling = timesampling;
            _bollingerLength = bollingerLength;
            _slowEma = slowEma;
            _fastEma = fastEma;
            _macdEma = macdEma;
            _macdSlope = macdSlope;
            _vwap = vwap;
            _vwapSlope = vwapSlope;
            _rsi = rsi;
            _rsiSlope = rsiSlope;
            _cmf = cmf;
            _cmfSlope = cmfSlope;
            _normalize = normalize;

            Encoding = $"Indicators-Boll({_bollingerLength})MACD({_slowEma},{_fastEma},{_macdEma},{_macdSlope})VWAP({_vwap},{_vwapSlope})RSI({_rsi},{_rsiSlope})CMF({_cmf},{_cmfSlope}),n{_normalize}-v{_version}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            //var ordered = HeikinAshiCandlesStockFeatureExtractor.ToHeikinAshiCandles(data.Select(x => x.Data)
            //    .OrderByDescending(x => x.Date)).Select(x => (x, 1.0f));

            var featureVector = new List<double>();
            var boll = new double[_timeSampling];
            var macdVals = new double[_timeSampling];
            var macdSlopeVals = new double[_timeSampling];
            var vwapVals = new double[_timeSampling];
            var vwapSlopeVals = new double[_timeSampling];
            var rsiVals = new double[_timeSampling];
            var rsiSlopeVals = new double[_timeSampling];
            var cmfVals = new double[_timeSampling];
            var cmfSlopeVals = new double[_timeSampling];

            for (int i = 1; i <= _timeSampling; ++i)
            {
                boll[i - 1] = CalculateBollinger(data, _bollingerLength / i);
                CalculateMACD(data, _slowEma / i, _fastEma / i, _macdEma / i, _macdSlope,
                    out macdVals[i - 1], out macdSlopeVals[i - 1]);
                CalculateVWAP(data, _vwap / i, _vwapSlope,
                    out vwapVals[i - 1], out vwapSlopeVals[i - 1]);
                CalculateRSI(data, _rsi / i, _rsiSlope,
                    out rsiVals[i - 1], out rsiSlopeVals[i - 1]);
                CalculateCMF(data, _cmf / i, _cmfSlope,
                    out cmfVals[i - 1], out cmfSlopeVals[i - 1]);
            }

            if (_normalize)
            {
                var normalized = Normalize(boll)
                       .Concat(Normalize(macdVals))
                       .Concat(Normalize(macdSlopeVals))
                       .Concat(Normalize(vwapVals))
                       .Concat(Normalize(vwapSlopeVals))
                       .Concat(Normalize(rsiVals))
                       .Concat(Normalize(rsiSlopeVals))
                       .Concat(Normalize(cmfVals))
                       .Concat(Normalize(cmfSlopeVals)).ToArray();
                return normalized;
            }

            return boll
                       .Concat(macdVals)
                       .Concat(macdSlopeVals)
                       .Concat(vwapVals)
                       .Concat(vwapSlopeVals)
                       .Concat(rsiVals)
                       .Concat(rsiSlopeVals)
                       .Concat(cmfVals)
                       .Concat(cmfSlopeVals).ToArray();

        }

        private double CalculateBollinger(IEnumerable<(StockData Data, float Weight)> data,
            int length)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).Take(length).ToList();
            var mean = ordered.Sum(x => x.Data.Average) / (decimal)length;
            var stdev = Math.Sqrt(ordered.Sum(x => Math.Pow((double)(x.Data.Average - mean), 2.0)) / (double)length);

            return (double)(ordered.First().Data.Average - mean) / stdev;
        }

        private void CalculateMACD(IEnumerable<(StockData Data, float Weight)> data,
            int slowEmaLength, int fastEmaLength, int macdEmaLength, int macdSlopeLength,
            out double macd, out double macdSlope)
        {
            if (data.Count() < slowEmaLength + macdEmaLength + macdSlopeLength)
            {
                macd = 0.0;
                macdSlope = 0.0;
                return;
            }

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            var macdLine = new double[macdEmaLength + macdSlopeLength];
            double maxVal = 0.0001;
            for (int i = 0; i < macdLine.Length; ++i)
            {
                double slowEma = (double)ordered.Skip(i).Take(slowEmaLength).Average(x => x.Data.Average);
                double fastEma = (double)ordered.Skip(i).Take(fastEmaLength).Average(x => x.Data.Average);
                if (maxVal < slowEma)
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
            if (double.IsNaN(macd))
            {
                macd = 0.0;
            }
            var kernel = GetNormalizedWavelet(macdSlopeLength);
            macdSlope = 0.0;
            for (int i = 0; i < macdSlopeLength; ++i)
            {
                macdSlope += macdHistogram[i] * kernel[i];
            }
        }

        private void CalculateVWAP(IEnumerable<(StockData Data, float Weight)> data,
            int vwapLength, int vwapSlopLength,
            out double vwap, out double vwapSlope)
        {
            if (data.Count() < vwapLength + vwapSlopLength)
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
                if (avgLosses < 0.01)
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
            for (int i = 0; i < input.Length; ++i)
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
            for (int i = 0; i < length; ++i)
            {
                double t = i - center;
                kernel[i] = f * (1.0 - Math.Pow(t / sigma, 2.0)) *
                    Math.Pow(e, -1.0 * t * t / (2.0 * sigma * sigma));
            }

            return kernel;
        }
    }
}
