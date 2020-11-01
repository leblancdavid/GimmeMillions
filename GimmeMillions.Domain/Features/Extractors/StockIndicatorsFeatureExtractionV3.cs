using Accord;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class StockIndicatorsFeatureExtractionV3 : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _bollingerLength  = 40;
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
        private int _version = 3;
        private int _pivotKernel = 5;

        private Dictionary<int, double[]> _kernels;
        private double[] _fibonacciLevels;
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
            int pivotKernel = 5)
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
            _pivotKernel = pivotKernel;

            _kernels = new Dictionary<int, double[]>();
            _kernels[_macdSlope] = GetNormalizedWavelet(_macdSlope);
            _kernels[_vwapSlope] = GetNormalizedWavelet(_vwapSlope);
            _kernels[_rsiSlope] = GetNormalizedWavelet(_rsiSlope);
            _kernels[_cmfSlope] = GetNormalizedWavelet(_cmfSlope);
            _fibonacciLevels = new double[] { 0.0, 0.382, 0.5, 0.618, 1.0, 1.618 };
            Encoding = $"Indicators-Pivots({_pivotKernel})Boll({_bollingerLength})MACD({_slowEma},{_fastEma},{_macdEma},{_macdSlope})VWAP({_vwap},{_vwapSlope})RSI({_rsi},{_rsiSlope})CMF({_cmf},{_cmfSlope}),-v{_version}";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var featureVector = new List<double>();
            var boll = new double[_timeSampling];
            var volume = new double[_timeSampling];
            var fibs = new List<double>();
            var volatility = new List<double>();
            var macdVals = new double[_timeSampling];
            var macdSlopeVals = new double[_timeSampling];
            var vwapVals = new double[_timeSampling];
            var vwapSlopeVals = new double[_timeSampling];
            var rsiVals = new double[_timeSampling];
            var rsiSlopeVals = new double[_timeSampling];
            var cmfVals = new double[_timeSampling];
            var cmfSlopeVals = new double[_timeSampling];

            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            for (int i = 1; i <= _timeSampling; ++i)
            {
                boll[i - 1] = CalculateBollinger(ordered, _bollingerLength / i);
                volume[i - 1] = GetVolumeChange(ordered, ordered.Count / (i + 1));
                fibs = fibs.Concat(GetFibonacciRings(ordered, ordered.Count / (i + 1))).ToList();
                volatility = volatility.Concat(GetVolatility(ordered, ordered.Count / (i + 1))).ToList();
                CalculateMACD(ordered, _slowEma / i, _fastEma / i, _macdEma / i, _macdSlope, 
                    out macdVals[i - 1], out macdSlopeVals[i - 1]);
                CalculateVWAP(ordered, _vwap / i, _vwapSlope, 
                    out vwapVals[i - 1], out vwapSlopeVals[i - 1]);
                CalculateRSI(ordered, _rsi / i, _rsiSlope, 
                    out rsiVals[i - 1], out rsiSlopeVals[i - 1]);
                CalculateCMF(ordered, _cmf / i, _cmfSlope, 
                    out cmfVals[i - 1], out cmfSlopeVals[i - 1]);
            }

            return macdVals.Concat(macdSlopeVals)
                .Concat(rsiVals).Concat(rsiSlopeVals)
                .Concat(vwapVals).Concat(vwapSlopeVals)
                .Concat(boll)
                .Concat(fibs)
                .Concat(volume)
                .Concat(cmfVals).Concat(cmfSlopeVals)
                .ToArray();

        }

        public double GetVolumeChange(List<(StockData Data, float Weight)> data, int length)
        {
            double averageVolume = data.Average(x => (double)x.Data.Volume);
            double volume = data.Take(length).Average(x => (double)x.Data.Volume);
            return volume / averageVolume;
        }

        public double[] GetVolatility(List<(StockData Data, float Weight)> data, int length)
        {
            double averageDayRange = data.Average(x => (double)x.Data.PercentDayRange);
            double averageBody = data.Average(x => (double)Math.Abs(x.Data.PercentDayChange));
            double averageBottom = data.Average(x => (double)x.Data.BottomWickPercent);
            double averageTop = data.Average(x => (double)Math.Abs(x.Data.TopWickPercent));
            var subData = data.Take(length).ToList();
            double range = subData.Average(x => (double)x.Data.PercentDayRange);
            double bodyRange = subData.Average(x => (double)Math.Abs(x.Data.PercentDayChange));
            double botRange = subData.Average(x => (double)x.Data.BottomWickPercent);
            double topRange = subData.Average(x => (double)Math.Abs(x.Data.TopWickPercent));

            var stats = new double[4];
            stats[0] = range / averageDayRange;
            stats[1] = bodyRange / averageBody;
            stats[2] = botRange / averageBottom;
            stats[3] = topRange / averageTop;

            return stats;
        }

        public double[] GetFibonacciRings(List<(StockData Data, float Weight)> data, int length)
        {
            var subData = data.Take(length).ToList();
            (double p, int i) max, min;
            max.p = 0.0;
            min.p = double.MaxValue;
            max.i = 0;
            min.i = 0;
            var pivots = GetPivots(subData);
            if (pivots.Count >= 3)
            {
                foreach(var p in pivots)
                {
                    if(p.p > max.p)
                    {
                        max = p;
                    }
                    if(p.p < min.p)
                    {
                        min = p;
                    }
                }
            }
            else
            {
                int i = 0;
                foreach(var d in subData)
                {
                    if ((double)d.Data.High > max.p)
                    {
                        max.p = (double)d.Data.High;
                        max.i = i;
                    }
                    if ((double)d.Data.Low < min.p)
                    {
                        min.p = (double)d.Data.Low;
                        min.i = i;
                    }
                    i++;
                }
            }

            var fibs = new double[3];
            double close = (double)data.First().Data.Close;
            if(max.p == min.p)
            {
                fibs[0] = 0.0;
            }
            else
            {
                fibs[0] = Math.Abs(max.p - close) / (max.p - min.p);

            }
            double fibRingRange = Math.Sqrt((max.p - min.p) * (max.p - min.p) +
                (double)(max.i - min.i) * (double)(max.i - min.i));

            if(fibRingRange < 0.01)
            {
                fibs[1] = 0.0;
                fibs[2] = 0.0;
            }
            else
            {
                double lowRing = Math.Sqrt((close - min.p) * (close - min.p) +
                (double)(min.i) * (double)(min.i));
                fibs[1] = lowRing / fibRingRange;

                double highRing = Math.Sqrt((close - max.p) * (close - max.p) +
                    (double)(max.i) * (double)(max.i));
                fibs[2] = highRing / fibRingRange;
            }
            
            //return fibLevel;
            return fibs;
        }

        private double CalculateBollinger(List<(StockData Data, float Weight)> data,
            int length)
        {
            var subData = data.Take(length);
            var mean = subData.Average(x => x.Data.Close);
            var stdev = Math.Sqrt(subData.Sum(x => Math.Pow((double)(x.Data.Close - mean), 2.0)) / (double)length);

            return (double)(data.First().Data.Close - mean) / stdev;
        }

        private void CalculateMACD(List<(StockData Data, float Weight)> data,
            int slowEmaLength, int fastEmaLength, int macdEmaLength, int macdSlopeLength,
            out double macd, out double macdSlope)
        {
            if (data.Count() < slowEmaLength + macdEmaLength + macdSlopeLength)
            {
                macd = 0.0;
                macdSlope = 0.0;
                return;
            }

            var macdLine = new double[macdEmaLength + macdSlopeLength];
            double maxVal = 0.0001;
            for (int i = 0; i < macdLine.Length; ++i)
            {
                double slowEma = (double)data.Skip(i).Take(slowEmaLength).Average(x => x.Data.Close);
                double fastEma = (double)data.Skip(i).Take(fastEmaLength).Average(x => x.Data.Close);
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
            var kernel = _kernels[macdSlopeLength];// GetNormalizedWavelet(macdSlopeLength);
            macdSlope = 0.0;
            for(int i = 0; i < macdSlopeLength; ++i)
            {
                macdSlope += macdHistogram[i] * kernel[i];
            }
        }

        private void CalculateVWAP(List<(StockData Data, float Weight)> data,
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
            double maxVal = 0.0001;
            for (int i = 0; i < vwapHistogram.Length; ++i)
            {
                var samples = data.Skip(i).Take(vwapLength);
                var totalVolume = samples.Sum(x => x.Data.Volume);
                if(totalVolume < 0.0001m)
                {
                    vwapHistogram[i] = (double)data[i].Data.Close;
                }
                else
                {
                    var vwapAvg = (double)samples.Sum(x => x.Data.Close * x.Data.Volume) / (double)totalVolume;
                    vwapHistogram[i] = ((double)data[i].Data.Close - vwapAvg);
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

            var kernel = _kernels[vwapSlopLength];//GetNormalizedWavelet(vwapSlopLength);
            vwapSlope = 0.0;
            for (int i = 0; i < vwapSlopLength; ++i)
            {
                vwapSlope += vwapHistogram[i] * kernel[i] / maxVal;
            }
        }
        
        private void CalculateRSI(List<(StockData Data, float Weight)> data,
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
            var kernel = _kernels[rsiSlopeLength];//GetNormalizedWavelet(rsiSlopeLength);
            rsiSlope = 0.0;
            for (int i = 0; i < rsiSlopeLength; ++i)
            {
                rsiSlope += rsiHistogram[i] * kernel[i];
            }
        }

        private void CalculateCMF(List<(StockData Data, float Weight)> data,
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
            double maxVal = 0.0001;
            for (int i = 0; i < cmfHistogram.Length; ++i)
            {
                var samples = data.Skip(i).Take(cmfLength);
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
            var kernel = _kernels[cmfSlopeLength];//GetNormalizedWavelet(cmfSlopeLength);
            cmfSlope = 0.0;
            for (int i = 0; i < cmfSlopeLength; ++i)
            {
                cmfSlope += cmfHistogram[i] * kernel[i] / maxVal;
            }
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


        private List<(double p, int i)> GetPivots(List<(StockData Data, float Weight)> data)
        {
            var pivots = new List<(double p, int i)>();
            int k = _pivotKernel / 2;
            for(int i = k; i < data.Count - k; ++i)
            {
                bool resistance = true;
                for(int j = i - k; j < i + k; ++j)
                {
                    if (j == i)
                        continue;
                    if(data[i].Data.High < data[j].Data.High)
                    {
                        resistance = false;
                        break;
                    }
                }
                if(resistance)
                {
                    pivots.Add(((double)data[i].Data.High, i));
                }

                bool support = true;
                for (int j = i - k; j < i + k; ++j)
                {
                    if (j == i)
                        continue;
                    if (data[i].Data.Low > data[j].Data.Low)
                    {
                        support = false;
                        break;
                    }
                }
                if (support)
                {
                    pivots.Add(((double)data[i].Data.Low, i));
                }
            }
            return pivots;
        }
    }
}
