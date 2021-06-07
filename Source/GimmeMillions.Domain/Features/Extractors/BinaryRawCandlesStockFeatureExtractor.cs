using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class BinaryRawCandlesStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }

        public int OutputLength { get; private set; }

        public BinaryRawCandlesStockFeatureExtractor()
        {
            Encoding = "BinaryRawFeatures";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            double averageVolume = ordered.Average(x => (double)x.Data.Volume);
            double averageBottom = data.Average(x => (double)x.Data.BottomWickPercent);
            double averageTop = data.Average(x => (double)x.Data.TopWickPercent);
            
             var feature = new List<double>();
            foreach(var bar in ordered)
            {
                feature.Add((double)bar.Data.PercentPeriodChange > 0.0 ? 1.0 : 0.0);
                feature.Add((double)bar.Data.PercentChangeOpenToPreviousClose > 0.0 ? 1.0 : 0.0);
                feature.Add((double)bar.Data.BottomWickPercent > averageBottom ? 1.0 : 0.0);
                feature.Add((double)bar.Data.TopWickPercent > averageTop ? 1.0 : 0.0);
                feature.Add((double)bar.Data.Volume > averageVolume ? 1.0 : 0.0);
            }

            OutputLength = feature.Count;
            return feature.ToArray();
        }
    }
}
