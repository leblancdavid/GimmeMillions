using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class NormalizedHeikinAshiCandlesStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }

        public NormalizedHeikinAshiCandlesStockFeatureExtractor()
        {
            Encoding = "NormHeikinAshiFeatures";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = ToHeikinAshiCandles(data.Select(x => x.Data).OrderByDescending(x => x.Date));

            double minVolume = ordered.Min(x => (double)x.Volume);
            double maxVolume = ordered.Max(x => (double)x.Volume);
            double minBody = ordered.Min(x => (double)x.PercentPeriodChange);
            double maxBody = ordered.Max(x => (double)x.PercentPeriodChange); 
            double minGap = ordered.Min(x => (double)x.PercentChangeOpenToPreviousClose);
            double maxGap = ordered.Max(x => (double)x.PercentChangeOpenToPreviousClose);
            double maxBottom = ordered.Max(x => (double)x.BottomWickPercent);
            double maxTop = ordered.Max(x => (double)x.TopWickPercent);
            
             var feature = new List<double>();
            foreach(var bar in ordered)
            {
                feature.Add(((double)bar.PercentPeriodChange - minBody) / (maxBody - minBody));
                feature.Add(((double)bar.PercentChangeOpenToPreviousClose - minGap) / (maxGap - minGap));
                feature.Add((double)bar.BottomWickPercent / maxBottom);
                feature.Add((double)bar.TopWickPercent / maxTop);
                feature.Add(((double)bar.Volume - minVolume) / (maxVolume - minVolume));
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
