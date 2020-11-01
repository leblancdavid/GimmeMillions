using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
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

            //var trainer = new MarketFuturesTrainer(new DefaultStockRepository(stockSqlDb));
            //trainer.Train("C:\\Recommendations\\App\\RecommendationMaker\\Models\\MarketFutures");

            var trainer = new CatModelTrainer(new DefaultStockRepository(stockSqlDb), 
                new DefaultDatasetFilter(
                    maxPercentHigh: 40.0m, 
                maxPercentLow: 40.0m,
                minPrice: 2.0m,
                maxPrice: 30.0m,
                minVolume: 500000.0m));
            trainer.Train("C:\\Recommendations\\App\\RecommendationMaker\\Models\\CatSmallCaps");
        }

    }
}
