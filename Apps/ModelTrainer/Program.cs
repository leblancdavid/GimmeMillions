using CommandLine;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Database;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using Microsoft.EntityFrameworkCore;

namespace ModelTrainer
{
    class Program
    {
        public class Options
        {

            [Option('p', "pathToModel", Required = false, HelpText = "The path to the model file")]
            public string PathToModel { get; set; }
            [Option('s', "secret", Required = false, HelpText = "The secret for the Coinbase API")]
            public string ApiSecret { get; set; }
            [Option('k', "key", Required = false, HelpText = "The Key for the Coinbase API")]
            public string ApiKey { get; set; }

            [Option('x', "passphrase", Required = false, HelpText = "The Passphrase for the Coinbase API")]
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

            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite($"Data Source=C:\\Gimmillions\\Server\\gm.db");
            var context = new GimmeMillionsContext(optionsBuilder.Options);
            context.Database.Migrate();

            var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var trainer = new EgyptianMauModelTrainer(new DefaultStockRepository(stockSqlDb), 
                new StockSymbolsFile("nasdaq_screener.csv"),
                "C:\\Gimmillions\\Server\\td_access");
            trainer.Train("C:\\Gimmillions\\Server\\Resources\\Models\\EgyptianMau");

            //var trainer = new DonskoyModelTrainer(new DefaultStockRepository(stockSqlDb));
            //trainer.Train("C:\\Stocks\\Models\\Donskoy");
            //trainer.TrainCrypto("C:\\Stocks\\Models\\Donskoy", StockDataPeriod.Minute);
            //var service = new CoinbaseApiAccessService(secret, key, passphrase);
            //trainer.TrainCrypto($"{pathToModels}\\Donskoy", StockDataPeriod.Hour, service);


            //var trainer = new MarketFuturesTrainer(new DefaultStockRepository(stockSqlDb));
            //trainer.Train("C:\\Recommendations\\App\\RecommendationMaker\\Models\\MarketFutures");

            //var trainer = new CryptoCatModelTrainer(new DefaultStockRepository(stockSqlDb));
            //trainer.Train("C:\\Stocks\\Models\\CryptoCat\\CryptoCat");

            //var trainer = new CatModelTrainer(new DefaultStockRepository(stockSqlDb),
            //    new DefaultStockFilter(
            //        maxPercentHigh: 40.0m,
            //    maxPercentLow: 40.0m,
            //    minPrice: 5.0m,
            //    maxPrice: 50.0m,
            //    minVolume: 100000.0m));
            //trainer.Train("C:\\Recommendations\\App\\RecommendationMaker\\Models\\CatSmallCaps");
        }

    }
}
