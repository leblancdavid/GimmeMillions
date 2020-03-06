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

namespace ModelTrainer
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

            var startDate = new DateTime(2010, 1, 1);
            var endDate = new DateTime(2019, 6, 1);
            var dataset = datasetService.GetTrainingData(stock, startDate, endDate);

            int numTestExamples = 60;
            var testSet = dataset.Value.Skip(dataset.Value.Count() - numTestExamples);
            var trainingSet = dataset.Value.Take(dataset.Value.Count() - numTestExamples);

            model.Parameters.PcaRank = 128;
            model.Parameters.FeatureSelectionRank = model.Parameters.PcaRank * 10;
            model.Parameters.NumIterations = 0;
            model.Parameters.NumCrossValidations = 0;
            model.Parameters.NumOfTrees = 32;
            model.Parameters.NumOfLeaves = 8;
            model.Parameters.MinNumOfLeaves = 3;

            Console.WriteLine($"-=== Training ===-");
            Console.WriteLine($"Num Features: { model.Parameters.FeatureSelectionRank} \t PCA: { model.Parameters.PcaRank}");
            Console.WriteLine($"Number of Trees: { model.Parameters.NumOfTrees} \t Number of Leaves: { model.Parameters.NumOfLeaves}");
            Console.WriteLine($"Pca Rank: {model.Parameters.PcaRank}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            var trainingResult = model.Train(trainingSet, 0.0);
            stopwatch.Stop();

            Console.WriteLine($"-=== Training done ===-");
            Console.WriteLine($"Training time: {stopwatch.ElapsedMilliseconds / 60000.0}");
            if (trainingResult.IsFailure)
            {
                Console.WriteLine($"Training failed: {trainingResult.Error}");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"-=== Results ===-");
            Console.WriteLine($"Accuracy: {trainingResult.Value.Accuracy} \t Area under PR curve: {trainingResult.Value.AreaUnderPrecisionRecallCurve}");
            Console.WriteLine($"-=== Saving Model... ===-");
            model.Save(_pathToModels);

            Console.WriteLine($"-=== Testing Model... ===-");
            double accuracy = 0.0;
            foreach (var testExample in testSet)
            {
                var prediction = model.Predict(testExample.Input); 
                if ((prediction.IsSuccess && prediction.Value.PredictedLabel && testExample.Output.PercentDayChange > 0) ||
                    (prediction.IsSuccess && !prediction.Value.PredictedLabel && testExample.Output.PercentDayChange <= 0))
                {
                    accuracy++;
                }
            }

            Console.WriteLine($"Test Accuracy: {accuracy / numTestExamples}");
            Console.WriteLine($"-=========================================================================================-");

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
