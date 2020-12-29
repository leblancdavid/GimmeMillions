using CommandLine;
using CryptoLive.Accounts;
using CryptoLive.Notification;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
            [Option('a', "simulationAccount", Required = true, HelpText = "Simulated account file")]
            public string SimulatedAccountFile { get; set; }
            [Option('t', "timeInterval", Required = true, HelpText = "The interval time period (currently supported: 1h and 5m")]
            public string TimeInterval { get; set; }

        }

        static void Main(string[] args)
        {
            string pathToModels = "", secret = "", key = "", passphrase = "", simulationAccount = "";
            var period = StockDataPeriod.FiveMinute;
            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed<Options>(o =>
                  {
                      pathToModels = o.PathToModel;
                      secret = o.ApiSecret;
                      key = o.ApiKey;
                      passphrase = o.ApiPassphrase;
                      simulationAccount = o.SimulatedAccountFile;
                      if(o.TimeInterval == "1h")
                      {
                          period = StockDataPeriod.Hour;
                      }
                  });
            var service = new CoinbaseApiAccessService(secret, key, passphrase);
            var datasetService = GetCoinbaseIndicatorFeaturesBuySellSignalDatasetService(service, period, 100, 15);
            var model = new MLStockRangePredictorModel();
            model.Load($"{pathToModels}/Donskoy/Crypto{period}");

            var simulation = new SimulationCryptoAccountManager();
            if (!File.Exists(simulationAccount))
                simulation.SaveAccount(simulationAccount);
            else
                simulation.LoadAccount(simulationAccount);

            var notifiers = new MultiCryptoEventNotifier(new List<ICryptoEventNotifier>()
            {
                simulation,
                new LoggingCryptoEventNotifier($"buy_sell_signal_{period}.log")
            });

            var scanner = new CryptoRealtimeScanner(model, datasetService, notifiers, 90.0, 15.0);

            scanner.Scan();

            if(period == StockDataPeriod.Hour)
            {
                Run1hScan(scanner);
            }
            else if(period == StockDataPeriod.FiveMinute)
            {
                Run5mScan(scanner);
            }
        }

        private static void Run5mScan(ICryptoRealtimeScanner scanner)
        {
            var lastDigit = (DateTime.Now.Minute / 5) * 5;
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                var currentTime = DateTime.Now;
                if (currentTime.Minute % 5 == 0 && 
                    lastDigit != currentTime.Minute && 
                    currentTime.Second > 10) // Add a 10 second delay just to give it enough room
                {
                    scanner.Scan();
                    lastDigit = currentTime.Minute;
                }
            }
        }

        private static void Run1hScan(ICryptoRealtimeScanner scanner)
        {
            var lastDigit = (DateTime.Now.Hour);
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                var currentTime = DateTime.Now;
                if (currentTime.Minute == 0 &&
                    lastDigit != currentTime.Hour &&
                    currentTime.Second > 10) // Add a 10 second delay just to give it enough room
                {
                    scanner.Scan();
                    lastDigit = currentTime.Hour;
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
