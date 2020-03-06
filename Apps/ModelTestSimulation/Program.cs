using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Binary;
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

        static void Main(string[] args)
        {
            string dictionaryToUse = "FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries";
            string stock = "S";
            var datasetService = GetBoWFeatureDatasetService(dictionaryToUse);

            var model = new MLStockBinaryFastForestModel();

            var startDate = new DateTime(2018, 1, 1);
            var endDate = new DateTime(2018, 6, 1);
            var testSet = datasetService.GetTrainingData(stock, startDate, endDate);
            if(testSet.IsFailure || !testSet.Value.Any())
            {
                Console.WriteLine($"No data found for {stock} from {startDate.ToString("MM/dd/yyyy")} to {endDate.ToString("MM/dd/yyyy")}");
                return;
            }
            string encoding = testSet.Value.First().Input.Encoding;
            Console.WriteLine("-=== Loading Model ===-");
            var loadSuccess = model.Load(_pathToModels, stock, encoding);
            if (loadSuccess.IsFailure)
            {
                Console.WriteLine("Couldn't load the model!");
                return;
            }

            Console.WriteLine("-=== Simulating... ===-");
            var startingPrice = testSet.Value.First().Output.Open;
            var endingPrice = testSet.Value.Last().Output.Close;
            var totalPercentChange = (endingPrice - startingPrice) / startingPrice;
            Console.WriteLine($"Percent change over period {startDate.ToString("MM/dd/yyyy")} to {endDate.ToString("MM/dd/yyyy")}: {totalPercentChange * 100m}%");

            decimal currentMoney = 1000.0m;
            double accuracy = 0.0;
            foreach(var sample in testSet.Value)
            {
                var prediction = model.Predict(sample.Input);
                if (prediction.IsSuccess && prediction.Value.PredictedLabel)
                {
                    if(sample.Output.PercentDayChange > 0)
                    {
                        accuracy++;
                    }

                    //if(prediction.Value.Score < model.Metadata.AverageUpperProbability / 1.5f)
                    currentMoney = currentMoney * (1.0m + sample.Output.PercentDayChange / 100m);
                
                }
                else
                {
                    if (sample.Output.PercentDayChange < 0)
                    {
                        accuracy++;
                    }
                }

                Console.WriteLine($"{sample.Output.Date.ToString("MM/dd/yyyy")}, Actual: {sample.Output.PercentDayChange}%, Prediction: {prediction.Value.Probability}, Current money: ${currentMoney}");
            }

            Console.WriteLine("-=== Done ===-");
            Console.WriteLine($"Final accuracy: {accuracy / testSet.Value.Count()}");

            Console.ReadKey();
        }

        private static IFeatureDatasetService GetBoWFeatureDatasetService(string dictionaryToUse)
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary(dictionaryToUse);

            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var stocksRepo = new StockDataRepository(_pathToStocks);

            var cache = new FeatureJsonCache(_pathToCache);

            return new DefaultFeatureDatasetService(bow, articlesRepo, stocksRepo, cache);
        }
    }
}
