using CommandLine;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendationEvaluation
{
    class Program
    {
        public class Options
        {
            [Option('m', "model", Required = false, HelpText = "The type of model to predict")]
            public string Model { get; set; }

            [Option('p', "pathToModel", Required = true, HelpText = "The path to the model file")]
            public string PathToModel { get; set; }

            [Option('f', "database", Required = true, HelpText = "The database file to use")]
            public string DatabaseLocation { get; set; }

            [Option('s', "startDate", Required = false, HelpText = "The start date to begin evaluation")]
            public string StartDate { get; set; }

            [Option('e', "endDate", Required = false, HelpText = "The start date to begin evaluation")]
            public string EndDate { get; set; }

            [Option('u', "update", Required = false, HelpText = "Update the recommendations")]
            public bool Update { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Running recommendations... args: {string.Join(" ", args)}");
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();

            var stockList = new List<string>();
            IStockRecommendationSystem<FeatureVector> recommendationSystem = null;
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;
            string model = "aadvark";
            IStockRepository stocksRepo = null;
            IStockRecommendationRepository recommendationRepo = null;
            IEnumerable<string> stockSymbols = null;
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       optionsBuilder.UseSqlite($"Data Source={o.DatabaseLocation}");
                       var context = new GimmeMillionsContext(optionsBuilder.Options);
                       context.Database.Migrate();
                       var stockSqlDb = new SQLStockHistoryRepository(optionsBuilder.Options);
                       stocksRepo = new DefaultStockRepository(stockSqlDb);
                       recommendationRepo = new SQLStockRecommendationRepository(optionsBuilder.Options);
                       stockSymbols = stocksRepo.GetSymbols();
                       if (!string.IsNullOrEmpty(o.Model))
                       {
                           if (o.Model == "aadvark")
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

                       if (!string.IsNullOrEmpty(o.StartDate))
                       {
                           startDate = DateTime.Parse(o.StartDate);
                       }
                       if (!string.IsNullOrEmpty(o.EndDate))
                       {
                           endDate = DateTime.Parse(o.EndDate);
                       }

                       if(o.Update)
                       {
                           var currentDate = startDate;
                           while (currentDate <= endDate)
                           {
                               Console.WriteLine($"Updating recommendations for {currentDate}");
                               if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                               {
                                   recommendationSystem.GetRecommendationsFor(stockSymbols, currentDate);
                               }
                               currentDate = currentDate.AddDays(1.0);
                           }
                       }
                       
                   });

            if (stockSymbols == null)
                return;

            int maxNumDays = 30;
            var accuracyTable = new double[maxNumDays];
            int totalSamples = 0;
            foreach(var symbol in stockSymbols)
            {
                var recommendations = recommendationRepo.GetStockRecommendations(model, symbol);
                foreach(var r in recommendations)
                {
                    if (r.Prediction < 10.0m && r.Date < startDate)
                        continue;

                    var result = Evaluate(r, stocksRepo);
                    if (result == null)
                        continue;

                    for(int i = result.DaysToHitTarget; i < maxNumDays; ++i)
                    {
                        accuracyTable[i]++;
                    }
                    totalSamples++;
                }
            }

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter($"results.txt"))
            {
                for (int i = 0; i < maxNumDays; ++i)
                {
                    accuracyTable[i] /= totalSamples;
                    file.WriteLine(accuracyTable[i]);
                }
            }

        }

        private static RecommendationEvaluationResults Evaluate(StockRecommendation stockRecommendation,
            IStockRepository stocksRepo)
        {
            var stockData = stocksRepo.GetStocks(stockRecommendation.Symbol)
                .Where(x => x.Date >= stockRecommendation.Date)
                .OrderBy(y => y.Date);
            if(stockData == null)
            {
                return null;
            }

            int days = 0;
            var result = new RecommendationEvaluationResults(stockRecommendation);
            foreach (var sd in stockData)
            {
                if (sd.High > result.HighOverPeriod)
                {
                    result.HighOverPeriod = sd.High;
                }

                if (sd.High >= stockRecommendation.PredictedPriceTarget)
                {
                    result.DaysToHitTarget = days;
                    break;
                }
                days++;
            }

            return result;
        }
    }
}
