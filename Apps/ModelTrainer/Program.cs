using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Accord;
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
            string dictionaryToUse = "Google-M+L";
            string stock = "AMZN";
            var datasetService = GetBoWFeatureDatasetService(dictionaryToUse);

            //var model = new MLStockRandomFeatureFastTreeModel();
            var model = new AccordClassificationStockPredictor();
            var startDate = new DateTime(2005, 1, 1);
            var endDate = new DateTime(2020, 3, 1);
            var dataset = datasetService.GetTrainingData(stock, startDate, endDate);

            var filteredDataset = dataset.Value;
            int numTestExamples = 60;

            var testSet = filteredDataset.Skip(filteredDataset.Count() - numTestExamples);
            var trainingSet = filteredDataset.Take(filteredDataset.Count() - numTestExamples);

            //model.Parameters.PcaRank = 200;
            //model.Parameters.FeatureSelectionRank = 1000;
            //model.Parameters.NumIterations = 1;
            //model.Parameters.NumCrossValidations = 5;
            //model.Parameters.NumOfTrees = 50;
            //model.Parameters.NumOfLeaves = 10;
            //model.Parameters.MinNumOfLeaves = 1;

            //Console.WriteLine($"-=== Training ===-");
            //Console.WriteLine($"Num Features: { model.Parameters.FeatureSelectionRank} \t PCA: { model.Parameters.PcaRank}");
            //Console.WriteLine($"Number of Trees: { model.Parameters.NumOfTrees} \t Number of Leaves: { model.Parameters.NumOfLeaves}");
            //Console.WriteLine($"Pca Rank: {model.Parameters.PcaRank}");
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
            Console.WriteLine($"Positive Precision: {trainingResult.Value.PositivePrecision} \t Positive Recall: {trainingResult.Value.PositiveRecall}");
            Console.WriteLine($"Negative Precision: {trainingResult.Value.NegativePrecision} \t Negative Recall: {trainingResult.Value.NegativeRecall}");
            Console.WriteLine($"-=== Saving Model... ===-");
            //model.Save(_pathToModels);

            Console.WriteLine($"-=== Testing Model... ===-");
            double accuracy = 0.0;
            foreach (var testExample in testSet)
            {
                var prediction = model.Predict(testExample.Input);
               if ((prediction.PredictedLabel && testExample.Output.PercentDayChange > 0) ||
                    (!prediction.PredictedLabel && testExample.Output.PercentDayChange <= 0))
                {
                    accuracy++;
                    Console.WriteLine($"Good! Probability: {prediction.Probability}");

                }
               else
                {
                    Console.WriteLine($"Bad! Probability: {prediction.Probability}");
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
