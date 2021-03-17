using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class RawCandlesStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }

        public RawCandlesStockFeatureExtractor()
        {
            Encoding = "RawFeatures";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            double averageVolume = ordered.Average(x => (double)x.Data.Volume); 
            double averageBody = data.Average(x => (double)Math.Abs(x.Data.PercentPeriodChange));
            double averageGap = data.Average(x => (double)Math.Abs(x.Data.PercentChangeOpenToPreviousClose));
            double averageBottom = data.Average(x => (double)x.Data.BottomWickPercent);
            double averageTop = data.Average(x => (double)x.Data.TopWickPercent);
            
             var feature = new List<double>();
            foreach(var bar in ordered)
            {
                feature.Add((double)bar.Data.PercentPeriodChange / averageBody);
                feature.Add((double)bar.Data.PercentChangeOpenToPreviousClose / averageGap);
                feature.Add((double)bar.Data.BottomWickPercent / averageBottom);
                feature.Add((double)bar.Data.TopWickPercent / averageTop);
                feature.Add((double)bar.Data.Volume / averageVolume);
            }

            return feature.ToArray();
        }
    }
}
