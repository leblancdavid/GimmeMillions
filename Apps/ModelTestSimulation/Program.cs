using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Binary;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTestSimulation
{
    class Program
    {
        static string _pathToArticles = "../../../../Repository/Articles";
        static string _pathToDictionary = "../../../../Repository/Dictionaries";
        static string _pathToLanguage = "../../../../Repository/Languages";
        static string _pathToStocks = "../../../../Repository/Stocks";
        static string _pathToCache = "../../../../Repository/Cache";
        static string _pathToModels = "../../../../Repository/Models";
        static string _pathToRecommendationConfigs = "../../../../Repository/Recommendations";
        static string _pathToKeys = "../../../../Repository/Keys";

        static void Main(string[] args)
        {
            string dictionaryToUse = "USA";
            var datasetService = GetBoWFeatureDatasetService(dictionaryToUse);
            var recommendationSystem = new StockRecommendationSystem<FeatureVector>(datasetService, _pathToModels);
            var stockRepository = new StockDataRepository(_pathToStocks);

            //var stocks = new string[] { "F","INTC" };

            var stocks = new string[] { "F","INTC", "MSFT", "ATVI", "VZ", "S", "INVA", "LGND", "LXRX", "XBI",
             "IWM", "AMZN", "GOOG", "AAPL", "RAD", "WBA", "DRQ", "CNX", "BOOM", "FAST", "DAL", "ZNH", "ARNC",
             "AAL", "ORCL", "AMD", "MU", "INFY", "CAJ", "HPQ", "DRE", "NLY", "MPW", "C", "WFC",
            "HSBC", "BAC", "RY", "AXP", "FB", "DIS", "BHP", "BBL", "DD", "GOLD", "DUK", "EXC", "FE", "EIX",
            "CMS", "MCD", "SBUX", "LOW", "HMC", "HD", "GM", "ROST", "BBY", "MAR", "KO", "PEP", "GIS", "GE", "ET",
            "T", "PFE", "PBR", "GILD", "CSCO", "NOK", "MGM", "XOM", "HAL", "JPM", "CMCSA", "MS", "CVX", "PCG", "MRK",
            "V", "EBAY", "WMT", "LUV", "NKE", "JNJ", "SYF", "HLT", "CVS" };

            var stockAccess = new YahooFinanceStockAccessService(stockRepository, _pathToStocks);

            foreach (var stock in stocks)
            {
                Console.WriteLine($"-=== Loading model for {stock} ===-");
                //var model = new MLStockKernelEstimationSvmModel();
                var model = new MLStockKernelEstimationFastForestModel();
                //var model = new MLStockPeakKernelEstimationFastForestModel();
                //var model = new MLStockKernelEstimationSdcaModel();
                var loadResult = model.Load(_pathToModels, stock, "BoW-v2-USA");
                if (loadResult.IsFailure)
                {
                    Console.WriteLine($"-!!! Failed to load model for {stock} !!!-");
                    continue;
                }
                recommendationSystem.AddModel(model);

                stockAccess.UpdateStocks(stock);
            }

            //recommendationSystem.SaveConfiguration($"{_pathToRecommendationConfigs}/KernelSvm-config-v1");
            //recommendationSystem.SaveConfiguration($"{_pathToRecommendationConfigs}/KernelFFPeak-config-v1");
            recommendationSystem.SaveConfiguration($"{_pathToRecommendationConfigs}/KernelFF-config-v1");
            //recommendationSystem.SaveConfiguration($"{_pathToRecommendationConfigs}/KernelScda-config-v1");


            //recommendationSystem.LoadConfiguration($"{_pathToRecommendationConfigs}/KernelSvm-config-v1");
            //recommendationSystem.LoadConfiguration($"{_pathToRecommendationConfigs}/KernelFFPeak-config-v1");
            recommendationSystem.LoadConfiguration($"{_pathToRecommendationConfigs}/KernelFF-config-v1");
            //recommendationSystem.LoadConfiguration($"{_pathToRecommendationConfigs}/KernelScda-config-v1");
            var startDate = new DateTime(2019, 1, 1);
            var endDate = new DateTime(2020, 3, 18);
            var currentDate = startDate;
            decimal currentMoney = 1000.0m;

            Console.WriteLine($"-=== Testing recommendation model with ${currentMoney.ToString("#.##")} ===-");
            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    currentDate = currentDate.AddDays(1.0);
                    continue;
                }

                Console.WriteLine($"Current money: ${currentMoney.ToString("#.##")}");
                var recommendations = recommendationSystem.GetAllRecommendations(currentDate)
                    .Where(x => x.Prediction.PredictedLabel).Take(3).ToList();
                Console.Write("Investments: ");
                decimal leftover = currentMoney;
                decimal returnOnInvestment = 0.0m;
                foreach(var r in recommendations)
                {
                    //if (!r.Prediction.PredictedLabel)
                    //    break;

                    var stock = stockRepository.GetStock(r.Symbol, currentDate);
                    if(stock.IsFailure)
                    {
                        continue;
                    }

                    //decimal investAmmount = (decimal)r.RecommendedInvestmentPercentage * currentMoney;
                    decimal investAmmount = currentMoney / recommendations.Count();
                    Console.Write($"{r.Symbol} (p: {r.Prediction.Probability}): {investAmmount.ToString("#.##")} ({stock.Value.PercentDayChange.ToString("#.##")}%), ");
                    leftover -= investAmmount;
                    returnOnInvestment += investAmmount * (1.0m + stock.Value.PercentDayChange / 100m);
                }
                Console.Write("\n");
                currentMoney = leftover + returnOnInvestment;

                currentDate = currentDate.AddDays(1.0);
            }

            Console.WriteLine($"-=== Done testing recommendation model, ended with ${currentMoney.ToString("#.##")} ===-");
            Console.ReadLine();
        }

        private static IFeatureDatasetService<FeatureVector> GetBoWFeatureDatasetService(string dictionaryToUse)
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary(dictionaryToUse);

            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesAccess = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache(_pathToCache);

            return new DefaultFeatureDatasetService(bow, articlesAccess, stocksRepo, cache);
        }
    }
}
