using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class HeikinAshiCandlesStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        public int OutputLength { get; private set; }

        public HeikinAshiCandlesStockFeatureExtractor()
        {
            Encoding = "HeikinAshiFeatures";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = ToHeikinAshiCandles(data.Select(x => x.Data).OrderByDescending(x => x.Date));
            double averageVolume = ordered.Average(x => (double)x.Volume); 
            double averageBody = ordered.Average(x => (double)Math.Abs(x.PercentPeriodChange));
            double averageGap = ordered.Average(x => (double)Math.Abs(x.PercentChangeOpenToPreviousClose));
            double averageBottom = ordered.Average(x => (double)x.BottomWickPercent);
            double averageTop = ordered.Average(x => (double)x.TopWickPercent);
            
             var feature = new List<double>();
            foreach(var bar in ordered)
            {
                feature.Add((double)bar.PercentPeriodChange / averageBody);
                feature.Add((double)bar.PercentChangeOpenToPreviousClose / averageGap);
                feature.Add((double)bar.BottomWickPercent / averageBottom);
                feature.Add((double)bar.TopWickPercent / averageTop);
                feature.Add((double)bar.Volume / averageVolume);
            }

            return feature.ToArray();
        }

        public static IEnumerable<StockData> ToHeikinAshiCandles(IEnumerable<StockData> data)
        {
            var candles = data.ToList();
            var heikinCandles = new List<StockData>();
            if (!data.Any())
                return heikinCandles;
            decimal previousClose = data.First().Close;
            for (int i = 1; i < candles.Count; ++i)
            {
                var candle = candles[i];
                var previousCandle = candles[i - 1];
                decimal open = 0.5m * (previousCandle.Open + previousCandle.Close);
                decimal close = 0.25m * (candle.Open + candle.High + candle.Low + candle.Close);
                decimal high = (new decimal[] { open, close, candle.High }).Max();
                decimal low = (new decimal[] { open, close, candle.Low }).Min();
                heikinCandles.Add(new StockData(candle.Symbol, candle.Date, open, high, low, close, close, candle.Volume, previousClose));

                previousClose = close;
            }

            return heikinCandles;
        }
        
    }
}
