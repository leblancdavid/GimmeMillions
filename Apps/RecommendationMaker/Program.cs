using CommandLine;
using GimmeMillions.DataAccess.Clients.TDAmeritrade;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Database;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            [Option('d', "date", Required = false, HelpText = "The date to make prediction")]
            public string Date { get; set; }

            [Option('p', "pathToModel", Required = true, HelpText = "The path to the model file")]
            public string PathToModel { get; set; }

            [Option('f', "database", Required = true, HelpText = "The database file to use")]
            public string DatabaseLocation { get; set; }
            [Option('a', "td-access", Required = true, HelpText = "The ameritrade access api key")]
            public string TdApiKey { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Running recommendations... args: {string.Join(" ", args)}");
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            
            var stockList = new StockSymbolsFile("nasdaq_screener.csv").GetStockSymbols();
            IStockRecommendationSystem<FeatureVector> recommendationSystem = null;
            IStockRecommendationSystem<FeatureVector> futuresRecommendationSystem = null;
            var date = DateTime.Today;
            var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<Program>();
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       optionsBuilder.UseSqlite($"Data Source={o.DatabaseLocation}");
                       var context = new GimmeMillionsContext(optionsBuilder.Options);
                       context.Database.Migrate();
                       //var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);
                       var recommendationRepo = new SQLStockRecommendationHistoryRepository(optionsBuilder.Options, logger);

                       var stockAccess = new TDAmeritradeStockAccessService(new TDAmeritradeApiClient(o.TdApiKey), 
                           new StockSymbolsFile("nasdaq_screener.csv"));
                       recommendationSystem = RecommendationSystemFactory.GetLambkinRecommendationSystem(stockAccess, recommendationRepo,
                            $"{o.PathToModel}/Stocks", logger);
                       if (recommendationSystem == null)
                       {
                           Console.WriteLine($"Unable to retrieve stocks model at {o.PathToModel}/StocksModel");
                       }

                       futuresRecommendationSystem = RecommendationSystemFactory.GetLambkinRecommendationSystem(stockAccess, recommendationRepo,
                            $"{o.PathToModel}/Futures", logger);
                       if (futuresRecommendationSystem == null)
                       {
                           Console.WriteLine($"Unable to retrieve futures model at {o.PathToModel}/FuturesModel");
                       }

                       if (!string.IsNullOrEmpty(o.Date))
                       {
                           if (o.Date.ToLower() == "tomorrow")
                           {
                               date = date.AddDays(1.0);
                           }
                           else
                           {
                               date = DateTime.Parse(o.Date);
                           }
                       }



                   });

            if (recommendationSystem == null || futuresRecommendationSystem == null)
            {
                return;
            }

            RunFuturesRecommendations(futuresRecommendationSystem, date);
            //RunCryptoRecommendations(recommendationSystem, date);
            RunDailyRecommendations(recommendationSystem, stockList, date);

        }

        private static void RunFuturesRecommendations(IStockRecommendationSystem<FeatureVector> recommendationSystem, DateTime date)
        {
            IEnumerable<StockRecommendation> recommendations;
            var stockList = new List<string>()
            {
                "DIA", "QQQ", "SPY", "RUT"
            };

            recommendations = recommendationSystem.RunRecommendationsFor(stockList, date, null);

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"futures-predictions"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, " +
                         $"({Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                        $"Sentiment: {Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"High: {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}% - {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"Low: {Math.Round(r.LowPrediction, 2, MidpointRounding.AwayFromZero)}% - {Math.Round(r.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"Conf: {Math.Round(r.Confidence, 2, MidpointRounding.AwayFromZero)}%";
                    Console.WriteLine(text);
                    //if(i < keepTop)
                    //{
                    file.WriteLine(text);
                    //s}
                    ++i;
                }
            }
        }

        private static void RunDailyRecommendations(IStockRecommendationSystem<FeatureVector> recommendationSystem,
            IEnumerable<string> stockList,
            DateTime date)
        {
            IEnumerable<StockRecommendation> recommendations;
            var filter = new DefaultStockFilter(minPrice: 10.0m);
            if (stockList.Any())
            {
                recommendations = recommendationSystem.RunRecommendationsFor(stockList, date, filter);
            }
            else
            {
                recommendations = recommendationSystem.RunAllRecommendations(date, filter);
            }

            recommendations = recommendations.OrderByDescending(x => x.Sentiment).ToList();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"long-predictions"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, " +
                         $"({Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                        $"Sentiment: {Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"High: {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}% - {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"Low: {Math.Round(r.LowPrediction, 2, MidpointRounding.AwayFromZero)}% - {Math.Round(r.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"Conf: {Math.Round(r.Confidence, 2, MidpointRounding.AwayFromZero)}%";
                    Console.WriteLine(text);
                    //if(i < keepTop)
                    //{
                    file.WriteLine(text);
                    //s}
                    ++i;
                }
            }

            recommendations = recommendations.OrderBy(x => x.Sentiment).ToList();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"short-predictions"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, " +
                         $"({Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                        $"Sentiment: {Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"High: {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}% - {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"Low: {Math.Round(r.LowPrediction, 2, MidpointRounding.AwayFromZero)}% - {Math.Round(r.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"Conf: {Math.Round(r.Confidence, 2, MidpointRounding.AwayFromZero)}%";
                    Console.WriteLine(text);
                    //if(i < keepTop)
                    //{
                    file.WriteLine(text);
                    //s}
                    ++i;
                }
            }
        }

        private static List<string> GetStockSymbolsFromWatchlistFile(string file, int lineSkip = 4)
        {
            var symbols = new List<string>();
            if (!File.Exists(file))
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
                    if (!ticker[0].Any(x => !char.IsLetter(x) || !char.IsUpper(x)))
                        symbols.Add(ticker[0]);
                }
            }
            return symbols;
        }
    }
}
