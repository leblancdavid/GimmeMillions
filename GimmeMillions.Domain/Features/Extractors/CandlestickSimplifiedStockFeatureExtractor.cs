using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickSimplifiedStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        private int _version = 1;
        public string Encoding { get; set; }

        public CandlestickSimplifiedStockFeatureExtractor(int version = 1)
        {
            _version = version;
            Encoding = $"CandlestickSimplified-v{_version}";

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
            var feature = new double[stocks.Count()];
            int index = 0;
            foreach(var stock in ordered)
            {
                var normalizedVolume = stock.Data.Volume / (averageVolume + 1.0m);
                if(stock.Data.PercentDayChange > 0.0m)
                {
                    feature[index] = (double)(normalizedVolume * 
                        (stock.Data.High - stock.Data.Close) / 
                        (stock.Data.High - stock.Data.Low + 0.1m));
                }
                else
                {
                    feature[index] = (double)(normalizedVolume * 
                        (stock.Data.Low - stock.Data.Open) / 
                        (stock.Data.High - stock.Data.Low + 0.1m));
                }
                index++;
            }

            return feature;
            //return Normalize(feature);
        }

        private double[] Normalize(double[] feature)
        {
            var output = new double[feature.Length];
            double stdev = Math.Sqrt(feature.Sum(x => Math.Pow(x, 2)));
            if (stdev < 0.001)
            {
                stdev = 1.0;
            }

            for (int i = 0; i < feature.Length; ++i)
            {
                output[i] = (feature[i]) / stdev;
            }

            return output;
        }

    }
}
