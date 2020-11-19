using GimmeMillions.Database;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DNNTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite($"Data Source=C:\\Stocks\\gm.db");
            var context = new GimmeMillionsContext(optionsBuilder.Options);
            context.Database.Migrate();

            var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var trainer = new MarketFuturesTrainer(new DefaultStockRepository(stockSqlDb));
            trainer.Train("C:\\Recommendations\\App\\RecommendationMaker\\Models\\MarketFutures");

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
