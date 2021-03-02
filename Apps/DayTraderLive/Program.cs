using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;
using System.Threading;

namespace DayTraderLive
{
    class Program
    {
        static void Main(string[] args)
        {
            var datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(StockDataPeriod.FifteenMinute, 12, 80, 9);
            var model = new MLStockRangePredictorModel();
            //model.Load($"Models/DayTrader_15m");
            model.Load($"Models/DayTrader_5m");

            var scanner = new DayTradeFuturesScanner(model, datasetService);

            //Run15mScan(scanner);
            Run5mScan(scanner);
        }

        private static void Run15mScan(DayTradeFuturesScanner scanner)
        {
            var lastDigit = (DateTime.Now.Minute / 15) * 15;
            scanner.Scan();
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                //scanner.Scan();
                //Thread.Sleep(1000);
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

        private static void Run5mScan(DayTradeFuturesScanner scanner)
        {
            var lastDigit = (DateTime.Now.Minute / 5) * 5;
            scanner.Scan();
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                //scanner.Scan();
                //Thread.Sleep(1000);
                var currentTime = DateTime.Now;
                if (currentTime.Minute % 5 == 0 &&
                    lastDigit != currentTime.Minute &&
                    currentTime.Second > 30) // Add a 30 second delay just to give it enough room
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
