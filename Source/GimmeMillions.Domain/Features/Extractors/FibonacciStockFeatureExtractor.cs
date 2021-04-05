using GimmeMillions.Domain.Features.Extractors;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class FibonacciStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private decimal _fibThreshold = 0.025m;

        public FibonacciStockFeatureExtractor(decimal fibThreshold = 0.025m)
        {
            Encoding = "FibonacciFeatures";
            _fibThreshold = fibThreshold;
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();

            var fib = GetOptimalFibonacciRetracement(ordered);
            var feature = new List<double>();
            if(fib == null || !ordered.Any())
            {
                return null;
            }

            var volumes = GetFibonacciVolumes(fib, ordered);
            var lastbar = ordered.First();
            decimal distance;
            var fibIndex = fib.NearestFibonacci(lastbar.Data, out distance);
            feature.Add((double)fib.GetFibonacciValue(lastbar.Data));
            feature.Add((double)distance);
            feature.Add(volumes[fibIndex]);

            feature = feature.Concat(volumes).ToList();
            return feature.ToArray();
        }

        private double[] GetFibonacciVolumes(FibonacciRetracement fib, List<(StockData Data, float Weight)> data)
        {
            double[] volumes = new double[fib.FibonacciRatios.Length];
            foreach(var d in data)
            {
                decimal distance = 0.0m;
                var index = fib.NearestFibonacci(d.Data, out distance);

                volumes[index] += (double)(d.Data.Volume * Math.Abs(1.0m - distance));
            }

            //Normalize the volume
            double min = volumes.Min();
            double max = volumes.Max();
            for(int i = 0; i < volumes.Length; ++i)
            {
                volumes[i] = (volumes[i] - min) / (max - min);
            }

            return volumes;
        }

        private FibonacciRetracement GetOptimalFibonacciRetracement(List<(StockData Data, float Weight)> data)
        {
            var fibs = GetFibsCandidates(data);
            if (!fibs.Any())
                return null;

            decimal minPrice = fibs.Min(x => x.Low);
            decimal maxPrice = fibs.Max(x => x.High);
            decimal distanceThreshold = (maxPrice - minPrice) / 2.0m;

            var score = new int[fibs.Count];
            int bestScore = 0, bestIndex = 0;
            foreach(var d in data)
            {
                for(int i = 0; i < fibs.Count; ++i)
                {
                    if (fibs[i].High - fibs[i].Low < distanceThreshold)
                        continue;

                    decimal distance;
                    fibs[i].NearestFibonacci(d.Data, out distance);
                    if(distance < _fibThreshold)
                    {
                        score[i]++;
                        if(score[i] > bestScore)
                        {
                            bestScore = score[i];
                            bestIndex = i;
                        }
                    }
                }
            }

            return fibs[bestIndex];
        }

        private List<FibonacciRetracement> GetFibsCandidates(List<(StockData Data, float Weight)> data)
        {
            List<int> support, resistance;
            GetPivots(data, out resistance, out support);

            var fibs = new List<FibonacciRetracement>();

            foreach (var r in resistance)
            {
                foreach(var s in support)
                {
                    if(data[r].Data.High > data[s].Data.Low)
                    {
                        fibs.Add(new FibonacciRetracement(data[r].Data.High, data[s].Data.Low));
                    }
                }
            }

            return fibs;
        }
        
        private void GetPivots(List<(StockData Data, float Weight)> data,
            out List<int> resistance,
            out List<int> support)
        {
            resistance = new List<int>();
            support = new List<int>();

            for(int i = 2; i < data.Count - 2; ++i)
            {
                if(data[i].Data.High > data[i - 1].Data.High && 
                    data[i].Data.High > data[i - 2].Data.High &&
                    data[i].Data.High > data[i + 1].Data.High &&
                    data[i].Data.High > data[i + 2].Data.High)
                {
                    resistance.Add(i);
                }

                if (data[i].Data.Low < data[i - 1].Data.Low &&
                    data[i].Data.Low < data[i - 2].Data.Low &&
                    data[i].Data.Low < data[i + 1].Data.Low &&
                    data[i].Data.Low < data[i + 2].Data.Low)
                {
                    support.Add(i);
                }
            }
        }
    }
}
