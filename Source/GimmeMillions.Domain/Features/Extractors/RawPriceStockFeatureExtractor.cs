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

        public int OutputLength { get; private set; }
        public RawPriceStockFeatureExtractor()
        {
            Encoding = "RawPriceFeatures";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            var minVolume = ordered.Min(x => x.Data.Volume);
            var maxVolume = ordered.Max(x => x.Data.Volume);

            var minPrice = ordered.Min(x => x.Data.Low);
            var maxPrice = ordered.Max(x => x.Data.High);


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

                if (minPrice == maxPrice)
                {
                    feature.Add(0.0);
                }
                else
                {
                    feature.Add((double)((bar.Data.Open - minPrice) / (maxPrice - minPrice)));
                    feature.Add((double)((bar.Data.Close - minPrice) / (maxPrice - minPrice)));
                    feature.Add((double)((bar.Data.Low - minPrice) / (maxPrice - minPrice)));
                    feature.Add((double)((bar.Data.High - minPrice) / (maxPrice - minPrice)));
                }
            }

            return feature.ToArray();
        }
    }
}
