using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;
using System.Diagnostics;

namespace CryptoLive
{
    class Program
    {
        static void Main(string[] args)
        {
            var period = StockDataPeriod.FiveMinute;
            var datasetService = GetCoinbaseIndicatorFeaturesBuySellSignalDatasetService(period, 100, 15);
            var model = new MLStockRangePredictorModel();
            model.Load($"C:\\Stocks\\Models\\Donskoy\\Crypto{period}");

            var runner = new CryptoLiveRunner(model, datasetService);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            runner.Refresh();

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                if(stopWatch.Elapsed > TimeSpan.FromSeconds(30))
                {
                    runner.Refresh();
                    stopWatch.Restart();
                }
            }
        }


        private static IFeatureDatasetService<FeatureVector> GetCoinbaseIndicatorFeaturesBuySellSignalDatasetService(
          StockDataPeriod period,
          int numStockSamples = 40,
          int kernelSize = 9)
        {
            var stocksRepo = new CoinbaseApiAccessService();
            var extractor = new StockIndicatorsFeatureExtractionV2(10,
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
