using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;

namespace DayTraderLive
{
    class Program
    {
        static void Main(string[] args)
        {
            var datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(StockDataPeriod.FifteenMinute, 12, 80, 9);
            var model = new MLStockRangePredictorModel();
            model.Load($"Models/DayTrader_15m");

            var scanner = new DayTradeFuturesScanner(model, datasetService);

            Run15mScan(scanner);

        }

        private static void Run15mScan(DayTradeFuturesScanner scanner)
        {
            var lastDigit = (DateTime.Now.Minute / 15) * 15;
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                //scanner.Scan();

                var currentTime = DateTime.Now;
                if (currentTime.Minute % 15 == 0 &&
                    lastDigit != currentTime.Minute &&
                    currentTime.Second > 10) // Add a 10 second delay just to give it enough room
                {
                    scanner.Scan();
                    lastDigit = currentTime.Minute;
                }
            }
        }

        private static IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
            StockDataPeriod period,
            int timeSampling = 10,
            int numStockSamples = 40,
            int kernelSize = 9)
        {
            var stocksRepo = new AlpacaStockAccessService();
            var extractor = new StockIndicatorsFeatureExtractionV2(timeSampling,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize);
        }
    }
}
