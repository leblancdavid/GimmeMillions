using GimmeMillions.Domain.Stocks;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickStockFeatureExtractor : IStockFeatureExtractor
    {
        private int _version = 2;
        public string Encoding { get; set; }

        public CandlestickStockFeatureExtractor(int version = 1)
        {
            _version = version;
            Encoding = $"Candlestick-v{_version}";

        }

        public FeatureVector Extract(IEnumerable<StockData> stocks)
        {
            var feature = new FeatureVector();
            feature.Encoding = Encoding;
            if(!stocks.Any())
            {
                return feature;
            }

            var ordered = stocks.OrderBy(x => x.Date).ToList();
            var lastStock = ordered.Last();
            feature.Date = lastStock.Date;

            decimal average = (lastStock.Close + lastStock.Open + lastStock.Low + lastStock.High) / 4.0m;

            feature.Data = new float[stocks.Count() * 4];
            int index = 0;
            foreach(var stock in ordered)
            {
                feature.Data[index * 4] = (float)(stock.Open - average);
                feature.Data[index * 4 + 1] = (float)(stock.Close - average);
                feature.Data[index * 4 + 2] = (float)(stock.High - average);
                feature.Data[index * 4 + 3] = (float)(stock.Low - average);

                index++;
            }

            return feature;
        }
    }
}
