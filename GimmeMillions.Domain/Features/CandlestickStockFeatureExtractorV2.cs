using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickStockFeatureExtractorV2 : IFeatureExtractor<StockData>
    {
        private int _version = 2;
        private bool _normalize = true;
        public string Encoding { get; set; }

        public CandlestickStockFeatureExtractorV2(int version = 2)
        {
            _version = version;
            Encoding = $"Candlestick{_normalize}-v{_version}";

        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> stocks)
        {
            if(!stocks.Any())
            {
                return new double[0];
            }

            var ordered = stocks.OrderBy(x => x.Data.Date).ToList();
            var lastStock = ordered.Last();

            decimal averageVolume = ordered.Average(x => x.Data.Volume);
            var feature = new double[stocks.Count() * 4];
            int index = 0;
            foreach(var stock in ordered)
            {
                feature[index * 4] = (double)(stock.Data.PercentDayChange);
                feature[index * 4 + 1] = (double)(stock.Data.TopWickPercent);
                feature[index * 4 + 2] = (double)(stock.Data.BottomWickPercent);
                feature[index * 4 + 3] = (double)(stock.Data.Volume / (averageVolume + 1.0m));

                index++;
            }

            if (_normalize)
                return Normalize(feature);

            return feature;
        }

        private double[] Normalize(double[] feature)
        {
            var output = new double[feature.Length];
            double stdev = Math.Sqrt(feature.Sum(x => Math.Pow(x, 2)));
            if(stdev < 0.001)
            {
                stdev = 1.0;
            }

            for(int i = 0; i < feature.Length; ++i)
            {
                output[i] = (feature[i]) / stdev;
            }

            return output;
        }
    }
}
