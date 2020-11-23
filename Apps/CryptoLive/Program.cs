using CommandLine;
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
        public class Options
        {

            [Option('p', "pathToModel", Required = true, HelpText = "The path to the model file")]
            public string PathToModel { get; set; }
            [Option('s', "secret", Required = true, HelpText = "The secret for the Coinbase API")]
            public string ApiSecret { get; set; }
            [Option('k', "key", Required = true, HelpText = "The Key for the Coinbase API")]
            public string ApiKey { get; set; }

            [Option('x', "passphrase", Required = true, HelpText = "The Passphrase for the Coinbase API")]
            public string ApiPassphrase { get; set; }
        }

        static void Main(string[] args)
        {
            string pathToModels = "", secret = "", key = "", passphrase = "";
            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed<Options>(o =>
                  {
                      pathToModels = o.PathToModel;
                      secret = o.ApiSecret;
                      key = o.ApiKey;
                      passphrase = o.ApiPassphrase;
                  });
            var period = StockDataPeriod.FiveMinute;
            var service = new CoinbaseApiAccessService(secret, key, passphrase);
            var datasetService = GetCoinbaseIndicatorFeaturesBuySellSignalDatasetService(service, period, 100, 15);
            var model = new MLStockRangePredictorModel();
            model.Load($"{pathToModels}\\Donskoy\\Crypto{period}");

            var runner = new CryptoRealtimeScanner(model, datasetService, null);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            runner.Scan();

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                if(stopWatch.Elapsed > TimeSpan.FromSeconds(30))
                {
                    runner.Scan();
                    stopWatch.Restart();
                }
            }
        }


        private static IFeatureDatasetService<FeatureVector> GetCoinbaseIndicatorFeaturesBuySellSignalDatasetService(
            IStockAccessService accessService,
          StockDataPeriod period,
          int numStockSamples = 40,
          int kernelSize = 9)
        {
            var extractor = new StockIndicatorsFeatureExtractionV2(10,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false);
            return new BuySellSignalFeatureDatasetService(extractor, accessService,
                period, numStockSamples, kernelSize);
        }
    }
}
