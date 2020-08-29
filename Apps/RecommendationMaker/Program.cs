using CommandLine;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RecommendationMaker
{
    class Program
    {
        static string _pathToModels = "../../../../Repository/Models";
        static string _dbLocation = "C:/Databases/gm.db";

        public class Options
        {
            [Option('w', "watchlist", Required = false, HelpText = "The watchlist file to pick from for recommendations")]
            public string WatchlistFile { get; set; }
        }

        static void Main(string[] args)
        {
            var stockList = new List<string>();
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if(!string.IsNullOrEmpty(o.WatchlistFile))
                       {
                           stockList = GetStockSymbolsFromWatchlistFile(o.WatchlistFile);
                       }
                   });

            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite($"Data Source={_dbLocation}");
            var context = new GimmeMillionsContext(optionsBuilder.Options);
            context.Database.Migrate();
            var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);

            var stocksRepo = new DefaultStockRepository(stockSqlDb);
            var recommendationSystem = RecommendationSystemFactory.GetAadvarkRecommendationSystem(stocksRepo, _pathToModels);

            var date = DateTime.Today;
            //var date = DateTime.Today.AddDays(1.0);

            IEnumerable<StockRecommendation> recommendations;
            if(stockList.Any())
            {
                recommendations = recommendationSystem.GetRecommendationsFor(stockList, date, true);
            }
            else
            {
                recommendations = recommendationSystem.GetAllRecommendations(date, true);
            }

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"C:\\Recommendations\\{date.ToString("yyyy-MM-dd")}"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol},{r.Prediction.Probability}";
                    Console.WriteLine(text);
                    //if(i < keepTop)
                    //{
                        file.WriteLine(text);
                    //s}
                    ++i;
                }
            }
            foreach(var r in recommendations)
            {
                Console.WriteLine($"{r.Symbol}: {r.Prediction.Score} ({r.Prediction.Probability})");
            }

        }
        
        private static List<string> GetStockSymbolsFromWatchlistFile(string file, int lineSkip = 4)
        {
            var symbols = new List<string>();
            if(!File.Exists(file))
            {
                return symbols;
            }

            using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
            {
                string line;
                int i = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    ++i;
                    if (i <= lineSkip)
                        continue;
                    var ticker = line.Split(',');
                    if(!ticker[0].Any(x => !char.IsLetter(x) || !char.IsUpper(x)))
                        symbols.Add(ticker[0]);
                }
            }
            return symbols;
        }
    }
}
