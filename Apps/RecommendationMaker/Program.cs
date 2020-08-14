using CommandLine;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RecommendationMaker
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

            //var datasetService = GetHistoricalFeatureDatasetService(10, 20, FrequencyTimeframe.Daily, true);
            //var datasetService = GetHistoricalFeatureDatasetService(10, 10, FrequencyTimeframe.Weekly, false);
            var datasetService = GetCandlestickFeatureDatasetService(60, 5, true);
            var recommendationSystem = new CandlestickStockRecommendationSystem(datasetService, _pathToModels);

            var model = new MLStockFastForestCandlestickModel();
            model.Load(_pathToModels, "ANY_SYMBOL", "Indicators-MACD(32,16,12,7)VWAP(12,7)RSI(12,7)CMF(24,7),nFalse-v1_60d-5p_withComposite");
            recommendationSystem.AddModel(model);
            recommendationSystem.SaveConfiguration($"{_pathToRecommendationConfigs}/Indicators-MACD(32,16,12,7)VWAP(12,7)RSI(12,7)CMF(24,7),nFalse-v1_60d-5p_withComposite-config-v1");
            //var date = new DateTime(2019, 1, 11);
            var date = DateTime.Today;
            //var date = DateTime.Today.AddDays(1.0);

            IEnumerable<StockRecommendation> recommendations;
            if(stockList.Any())
            {
                recommendations = recommendationSystem.GetRecommendationsFor(stockList, date);
            }
            else
            {
                recommendations = recommendationSystem.GetAllRecommendations(date);
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

        private static CandlestickStockFeatureDatasetService GetCandlestickFeatureDatasetService(
           int numStockSamples = 40,
           int stockOutputPeriod = 3,
           bool includeComposites = false)
        {
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            //var candlestickExtractor = new CandlestickStockFeatureExtractor();
            //use default values for meow!
            var indictatorsExtractor = new StockIndicatorsFeatureExtraction(normalize: false);

            return new CandlestickStockFeatureDatasetService(indictatorsExtractor, stocksRepo,
                numStockSamples, stockOutputPeriod, includeComposites, cache, false);
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

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            int numArticlesDays = 10;
            return new DefaultFeatureDatasetService(bow, articlesAccess, stocksRepo, numArticlesDays, cache);
        }

        private static HistoricalFeatureDatasetService GetHistoricalFeatureDatasetService(int numArticleDays = 10,
            int numStockSamples = 10, FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily,
            bool includeComposites = false)
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary("USA");

            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var akmExtractor = new AKMBoWFeatureVectorExtractor(bow, 1000);
            akmExtractor.Load(_pathToModels);

            var stockExtractor = new CandlestickStockFeatureExtractor();

            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesAccess = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            return new HistoricalFeatureDatasetService(stockExtractor, akmExtractor, articlesAccess, stocksRepo,
                numArticleDays, numStockSamples, frequencyTimeframe, includeComposites, cache);
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
