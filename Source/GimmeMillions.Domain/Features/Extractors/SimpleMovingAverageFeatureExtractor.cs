using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class SimpleMovingAverageFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _quantization;

        public SimpleMovingAverageFeatureExtractor(int quantization = 5)
        {
            Encoding = "SMA";
            _quantization = quantization;
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();

            return GetMovingAverageFeature(ordered)
                .Concat(GetMovingVolumeFeature(ordered)).ToArray();
        }

        private List<double> GetMovingAverageFeature(List<(StockData Data, float Weight)> ordered)
        {
            decimal averagePrice = 0.0m;
            int index = 0;
            var movingAverages = new List<decimal>();
            foreach (var sample in ordered)
            {
                averagePrice += sample.Data.Average;
                index++;
                movingAverages.Add(averagePrice / index);
            }

            decimal max = 0.0m;
            var distanceToMovingAverage = new List<double>();
            for (int i = 0; i < ordered.Count; i += _quantization)
            {
                var d = ordered[i].Data.Average - movingAverages[i];
                if (Math.Abs(d) > max)
                {
                    max = Math.Abs(d);
                }
                distanceToMovingAverage.Add((double)d);
            }
            for (int i = 0; i < distanceToMovingAverage.Count; ++i)
            {
                distanceToMovingAverage[i] /= (double)max;
            }

            return distanceToMovingAverage;
        }

        private List<double> GetMovingVolumeFeature(List<(StockData Data, float Weight)> ordered)
        {
            decimal averageVolume = 0.0m;
            int index = 0;
            var movingAverages = new List<decimal>();
            foreach (var sample in ordered)
            {
                averageVolume += sample.Data.PriceNormalizedVolume;
                index++;
                movingAverages.Add(averageVolume / index);
            }

            decimal max = 0.0m;
            var distanceToMovingAverage = new List<double>();
            for (int i = 0; i < ordered.Count; i += _quantization)
            {
                var d = ordered[i].Data.PriceNormalizedVolume - movingAverages[i];
                if (Math.Abs(d) > max)
                {
                    max = Math.Abs(d);
                }
                distanceToMovingAverage.Add((double)d);
            }
            for (int i = 0; i < distanceToMovingAverage.Count; ++i)
            {
                distanceToMovingAverage[i] /= (double)max;
            }

            return distanceToMovingAverage;
        }
    }
}
