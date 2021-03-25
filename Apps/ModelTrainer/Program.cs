using CommandLine;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Database;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using Microsoft.EntityFrameworkCore;
using System;

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

            var trainer = new HimalayanModelTrainer(StockDataPeriod.Day,
                9, 100, 12, 0);

            trainer.Train("C:\\Users\\leblanc_d\\Documents\\Projects\\GimmeMillions\\Repository\\Models\\Himalayan\\Futures.dnn", 2000);
            //trainer.LoadModel("C:\\Users\\leblanc_d\\Documents\\Projects\\GimmeMillions\\Repository\\Models\\Himalayan\\Futures.dnn");
            trainer.Evaluate("model_results.csv", 500, "MSFT");
        }

    }
}
