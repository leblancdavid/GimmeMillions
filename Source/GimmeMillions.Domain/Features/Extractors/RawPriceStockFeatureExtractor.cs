using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class RawPriceStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }

        public RawPriceStockFeatureExtractor()
        {
            Encoding = "RawPriceFeatures";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            var minVolume = ordered.Min(x => x.Data.Volume);
            var maxVolume = ordered.Max(x => x.Data.Volume);

            var minClose = ordered.Min(x => x.Data.Close);
            var maxClose = ordered.Max(x => x.Data.Close);

            var minBottom = ordered.Min(x => x.Data.BottomWickPercent);
            var maxBottom = ordered.Max(x => x.Data.BottomWickPercent);

            var minTop = ordered.Min(x => x.Data.TopWickPercent);
            var maxTop = ordered.Max(x => x.Data.TopWickPercent);

            var feature = new List<double>();
            foreach(var bar in ordered)
            {
                if(minVolume == maxVolume)
                {
                    feature.Add(0.0);
                }
                else
                {
                    feature.Add((double)((bar.Data.Volume - minVolume) / (maxVolume - minVolume)));
                }

                if (minClose == maxClose)
                {
                    feature.Add(0.0);
                }
                else
                {
                    feature.Add((double)((bar.Data.Close - minClose) / (maxClose - minClose)));
                    //feature.Add((double)((bar.Data.Open - minClose) / (maxClose - minClose)));
                }

                if (minBottom == maxBottom)
                {
                    feature.Add(0.0);
                }
                else
                {
                    feature.Add((double)((bar.Data.BottomWickPercent - minBottom) / (maxBottom - minBottom)));
                }

                if (minTop == maxTop)
                {
                    feature.Add(0.0);
                }
                else
                {
                    feature.Add((double)((bar.Data.TopWickPercent - minTop) / (maxTop - minTop)));
                }
            }

            return feature.ToArray();
        }
    }
}
