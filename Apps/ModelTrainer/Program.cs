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
        public class Options
        {
            [Option('a', "td-access", Required = true, HelpText = "The ameritrade access api key")]
            public string TdApiKey { get; set; }
        }

        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite($"Data Source=gm.db");
            var context = new GimmeMillionsContext(optionsBuilder.Options);
            context.Database.Migrate();
            string apiKey = "";
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       apiKey = o.TdApiKey;
                   });

            //var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var trainer = new KoratModelTrainer(apiKey,
                new StockSymbolsFile("nasdaq_screener.csv"),
                StockDataPeriod.Day,
                9, 200, 12, 0, 10);

            //trainer.Train("C:\\Users\\leblanc_d\\Documents\\Projects\\GimmeMillions\\Repository\\Models\\Himalayan\\Futures.dnn", 20000);
            //730 = 365 x 2, basically two years of historical data should be good enough
            trainer.TrainFutures("Futures", 1000);
            //trainer.TrainStocks("StocksModel", 800);
            //trainer.LoadModel("FuturesModel");
            Console.WriteLine($"Total accuracy DIA: {trainer.Evaluate("trainingResults.csv", 500, "TSLA")}");
        }

    }
}
