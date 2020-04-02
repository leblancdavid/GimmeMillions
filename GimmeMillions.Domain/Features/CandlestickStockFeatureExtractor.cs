using GimmeMillions.Domain.Stocks;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        private int _version = 2;
        public string Encoding { get; set; }

        public CandlestickStockFeatureExtractor(int version = 1)
        {
            _version = version;
            Encoding = $"Candlestick-v{_version}";

        }

        public float[] Extract(IEnumerable<(StockData Data, float Weight)> stocks)
        {
            if(!stocks.Any())
            {
                return new float[0];
            }

            var ordered = stocks.OrderBy(x => x.Data.Date).ToList();
            var lastStock = ordered.Last();

            decimal average = (lastStock.Data.Close + lastStock.Data.Open + lastStock.Data.Low + lastStock.Data.High) / 4.0m;

            var feature = new float[stocks.Count() * 4];
            int index = 0;
            foreach(var stock in ordered)
            {
                feature[index * 4] = (float)(stock.Data.Open - average);
                feature[index * 4 + 1] = (float)(stock.Data.Close - average);
                feature[index * 4 + 2] = (float)(stock.Data.High - average);
                feature[index * 4 + 3] = (float)(stock.Data.Low - average);

                index++;
            }

            return feature;
        }
    }
}
