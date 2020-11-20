using CommandLine;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Database;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
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
            IStockRecommendationSystem<FeatureVector> futuresRecommendationSystem = null;
            var date = DateTime.Today;
            string model = "cat";
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

                       recommendationSystem = RecommendationSystemFactory.GetCatRecommendationSystem(stocksRepo, recommendationRepo,
                            $"{o.PathToModel}\\CatSmallCaps\\CatSmallCaps");
                       futuresRecommendationSystem = RecommendationSystemFactory.GetCatRecommendationSystem(stocksRepo, recommendationRepo,
                            $"{o.PathToModel}\\MarketFutures\\MarketFutures");
                       model = "cat";

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

            RunFuturesRecommendations(futuresRecommendationSystem, model, date);
            RunCryptoRecommendations(recommendationSystem, model, date);
            RunDailyRecommendations(recommendationSystem, stockList, model, date);

        }

        private static void RunCryptoRecommendations(IStockRecommendationSystem<FeatureVector> recommendationSystem,
            string model, DateTime date)
        {
            IEnumerable<StockRecommendation> recommendations;
            var stockList = new List<string>()
            {
                "BTC-USD",
                "ETH-USD",
                "XRP-USD",
                "LINK-USD",
            };

            recommendations = recommendationSystem.GetRecommendationsFor(stockList, date, null, true)
                .OrderByDescending(x => x.Sentiment).ToList();

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"C:\\Stocks\\{model}-crypto-{date.ToString("yyyy-MM-dd")}"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, " +
                         $"({Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                        $"gain: {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"high: {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"loss: {Math.Round(r.LowPrediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"low: {Math.Round(r.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}";
                    Console.WriteLine(text);
                    //if(i < keepTop)
                    //{
                    file.WriteLine(text);
                    //s}
                    ++i;
                }
            }
        }

        private static void RunFuturesRecommendations(IStockRecommendationSystem<FeatureVector> recommendationSystem,
            string model, DateTime date)
        {
            IEnumerable<StockRecommendation> recommendations;
            var stockList = new List<string>()
            {
                "DIA", "QQQ", "SPY"
            };

            recommendations = recommendationSystem.GetRecommendationsFor(stockList, date, null, true);

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"C:\\Stocks\\{model}-futures-{date.ToString("yyyy-MM-dd")}"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, " +
                         $"({Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                        $"gain: {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"high: {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"loss: {Math.Round(r.LowPrediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"low: {Math.Round(r.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}";
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
            string model,
            DateTime date)
        {
            IEnumerable<StockRecommendation> recommendations;
            var filter = new DefaultStockFilter(minVolume: 500000m, maxPercentHigh: 20.0m, maxPercentLow: 20.0m);
            if (stockList.Any())
            {
                recommendations = recommendationSystem.GetRecommendationsFor(stockList, date, filter, true);
            }
            else
            {
                recommendations = recommendationSystem.GetAllRecommendations(date, filter, true);
            }

            recommendations = recommendations.OrderByDescending(x => x.Sentiment).ToList();
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter($"C:\\Stocks\\{model}-long-{date.ToString("yyyy-MM-dd")}"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, " +
                        $"({Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                        $"gain: {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"high: {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"loss: {Math.Round(r.LowPrediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"low: {Math.Round(r.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}";
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
            new System.IO.StreamWriter($"C:\\Stocks\\{model}-short-{date.ToString("yyyy-MM-dd")}"))
            {
                string text = $"Stock recommendation for {date.ToString("MM/dd/yyyy")}:";
                Console.WriteLine(text);
                //file.WriteLine(text);
                int i = 0;
                foreach (var r in recommendations)
                {
                    text = $"{r.Symbol}, " +
                        $"({Math.Round(r.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                        $"gain: {Math.Round(r.Prediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"high: {Math.Round(r.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                        $"loss: {Math.Round(r.LowPrediction, 2, MidpointRounding.AwayFromZero)}%, " +
                        $"low: {Math.Round(r.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}";
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
