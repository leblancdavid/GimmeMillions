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
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite($"Data Source=gm.db");
            var context = new GimmeMillionsContext(optionsBuilder.Options);
            context.Database.Migrate();

            var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var trainer = new FuturesDayTradeModelTrainer(new DefaultStockRepository(stockSqlDb),
                new StockSymbolsFile("nasdaq_screener.csv"),
                "I12BJE0PV9ARIGTWWOPJGCGRWPBUJLRP");

            //trainer.Train("C:\\Users\\leblanc_d\\Documents\\Projects\\GimmeMillions\\Repository\\Models\\EgyptianMau\\DayTrader_15m", StockDataPeriod.FifteenMinute);
            trainer.Train("C:\\Users\\leblanc_d\\Documents\\Projects\\GimmeMillions\\Repository\\Models\\EgyptianMau\\DayTrader_5m", StockDataPeriod.FiveMinute);

            //var trainer = new EgyptianMauModelTrainer(new DefaultStockRepository(stockSqlDb), 
            //    new StockSymbolsFile("nasdaq_screener.csv"),
            //    "");
            //trainer.Train("Resources/Models/EgyptianMau");


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
