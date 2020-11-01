using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using Xunit;

namespace GimmeMillions.Domain.Tests.Features
{
    public class FeaturesTimingTests
    {
        [Fact]
        public void FeaturesTimingTest()
        {
            var randomStockData = new List<(StockData, float)>();
            var date = new DateTime();
            var rng = new Random();
            for(int i = 0; i < 1000; ++i)
            {
                randomStockData.Add((new StockData("TEST", date.AddDays(i),
                    (decimal)rng.NextDouble(),
                    (decimal)rng.NextDouble(),
                    (decimal)rng.NextDouble(),
                    (decimal)rng.NextDouble(),
                    (decimal)rng.NextDouble(),
                    (decimal)rng.NextDouble()), 1.0f));
            }

            int numStockSamples = 200;
            var v2Extractor = new StockIndicatorsFeatureExtractionV2(10,
                numStockSamples,
                (int)(numStockSamples * 0.8), 
                (int)(numStockSamples * 0.4), 
                (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            var v3Extractor = new StockIndicatorsFeatureExtractionV3(10,
                numStockSamples,
                (int)(numStockSamples * 0.8),
                (int)(numStockSamples * 0.4),
                (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5);

            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();
            for (int i = 0; i < 1000; i++)
            {
                var results = v2Extractor.Extract(randomStockData);
            }
            watch.Stop();
            var v2_t = watch.Elapsed;

            watch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var results = v3Extractor.Extract(randomStockData);
            }
            watch.Stop();
            var v3_t = watch.Elapsed;
        }
    }
}
