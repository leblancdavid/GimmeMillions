using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        private int _version = 2;
        private bool _normalize = true;
        public string Encoding { get; set; }

        public CandlestickStockFeatureExtractor(int version = 2)
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

            decimal average = (lastStock.Data.Close + lastStock.Data.Open + lastStock.Data.Low + lastStock.Data.High) / 4.0m;
            decimal averageVolume = ordered.Average(x => x.Data.Volume);
            var feature = new double[stocks.Count() * 5];
            int index = 0;
            foreach(var stock in ordered)
            {
                feature[index * 5] = (double)(stock.Data.Open - average);
                feature[index * 5 + 1] = (double)(stock.Data.Close - average);
                feature[index * 5 + 2] = (double)(stock.Data.High - average);
                feature[index * 5 + 3] = (double)(stock.Data.Low - average);
                feature[index * 5 + 4] = (double)(stock.Data.Volume / (averageVolume + 1.0m));

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
