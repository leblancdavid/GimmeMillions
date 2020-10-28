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
            optionsBuilder.UseSqlite($"Data Source=C:\\Recommendations\\App\\RecommendationMaker\\gm.db");
            var context = new GimmeMillionsContext(optionsBuilder.Options);
            context.Database.Migrate();

            var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var trainer = new MarketFuturesTrainer(new DefaultStockRepository(stockSqlDb));
            trainer.Train("C:\\Recommendations\\App\\RecommendationMaker\\Models\\MarketFutures");
        }

    }
}
