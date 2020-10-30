using CommandLine;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
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
        
        public class Options
        {
            [Option('w', "watchlist", Required = false, HelpText = "The watchlist file to pick from for recommendations")]
            public string WatchlistFile { get; set; }

            [Option('m', "model", Required = false, HelpText = "The type of model to predict")]
            public string Model { get; set; }
            
            [Option('d', "date", Required = false, HelpText = "The date to make prediction")]
            public string Date { get; set; }

            [Option('p', "pathToModel", Required = true, HelpText = "The path to the model file")]
            public string PathToModel { get; set; }

            [Option('f', "database", Required = true, HelpText = "The database file to use")]
            public string DatabaseLocation { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Running recommendations... args: {string.Join(" ", args)}");
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();

            var stockList = new List<string>();
            IStockRecommendationSystem<FeatureVector> recommendationSystem = null;
            var date = DateTime.Today;
            string model = "aadvark";
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       optionsBuilder.UseSqlite($"Data Source={o.DatabaseLocation}");
                       var context = new GimmeMillionsContext(optionsBuilder.Options);
                       context.Database.Migrate();
                       var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);
                       var stocksRepo = new DefaultStockRepository(stockSqlDb);
                       var recommendationRepo = new SQLStockRecommendationRepository(optionsBuilder.Options);

                       if (!string.IsNullOrEmpty(o.WatchlistFile))
                       {
                           stockList = GetStockSymbolsFromWatchlistFile(o.WatchlistFile);
                       }

                       if(!string.IsNullOrEmpty(o.Model))
                       {
                           if(o.Model == "aadvark")
                           {
                               recommendationSystem = RecommendationSystemFactory.GetAadvarkRecommendationSystem(stocksRepo, recommendationRepo, o.PathToModel);
                           }
                           else
                           {
                               recommendationSystem = RecommendationSystemFactory.GetBadgerRecommendationSystem(stocksRepo, recommendationRepo, o.PathToModel);
                               model = "badger";
                           }
                       }
                       else
                       {
                           recommendationSystem = RecommendationSystemFactory.GetAadvarkRecommendationSystem(stocksRepo, recommendationRepo, o.PathToModel);
                       }

                       if(!string.IsNullOrEmpty(o.Date))
                       {
                           date = DateTime.Parse(o.Date);
                       }


                       
                   });


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
            new System.IO.StreamWriter($"C:\\Stocks\\{model}-{date.ToString("yyyy-MM-dd")}"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}%, pt: {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}";
                    Console.WriteLine(text);
                    //if(i < keepTop)
                    //{
                        file.WriteLine(text);
                    //s}
                    ++i;
                }
            }
            //foreach(var r in recommendations)
            //{
            //    Console.WriteLine($"{r.Symbol}: {r.Prediction.Score} ({r.Prediction.Probability})");
            //}

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
