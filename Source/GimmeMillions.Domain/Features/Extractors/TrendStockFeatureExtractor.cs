using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class TrendStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        public int MaxLength { get; private set; }
        public bool IncludeVolume { get; private set; }
        public TrendStockFeatureExtractor(int maxLength, bool includeVolume = true)
        {
            Encoding = "TrendFeatures";
            MaxLength = maxLength;
            IncludeVolume = includeVolume;
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).Take(MaxLength).ToList();
            int i = 0;
            double sum = 0.0;
            var feature = new double[ordered.Count];
            foreach(var d in ordered)
            {
                sum += (double)d.Data.PercentChangeFromPreviousClose;
                feature[i] = sum / (double)(i + 1);
                i++;
            }

            if (IncludeVolume)
            {
                sum = 0.0;
                var averageVolume = ordered.Average(x => x.Data.Volume);
                var volumeTrend = new double[ordered.Count - 1];
                for(int j = 0; j < volumeTrend.Length; ++j)
                {
                    sum += (double)((ordered[j + 1].Data.Volume - ordered[j].Data.Volume) / averageVolume);
                    volumeTrend[j] = sum / (double)(j + 1);
                }

                feature = feature.Concat(volumeTrend).ToArray();
            }
            return feature;
        }
    }
}
