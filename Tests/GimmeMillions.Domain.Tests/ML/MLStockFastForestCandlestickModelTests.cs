using FluentAssertions;
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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.Domain.Tests.ML
{
    public class MLStockFastForestCandlestickModelTests
    {
        private readonly string _pathToArticles = "../../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../../Repository/Languages";
        private readonly string _pathToStocks = "../../../../../Repository/Stocks";
        private readonly string _pathToCache = "../../../../../Repository/Cache";
        private readonly string _pathToModels = "../../../../../Repository/Models";
        private readonly string _pathToKeys = "../../../../../Repository/Keys";

        [Fact]
        public void ShouldTrainUsingDailyCandlestickFeatures()
        {
            var datasetService = GetHistoricalFeatureDatasetService(10, 20, FrequencyTimeframe.Daily, true);
            var model = new MLStockFastForestCandlestickModel();
            model.Parameters.NumCrossValidations = 2;
            model.Parameters.NumOfTrees = 200;
            model.Parameters.NumOfLeaves = 20;
            model.Parameters.MinNumOfLeaves = 100;

            var endTrainingData = new DateTime(2019, 1, 1);
            var dataset = datasetService.GetAllTrainingData(new DateTime(2015, 1, 30), endTrainingData);
            dataset.Any().Should().BeTrue();

            var trainingResults = model.Train(dataset, 0.1);

            model.Save(_pathToModels);
            var testDataset = datasetService.GetAllTrainingData(endTrainingData, DateTime.Today);
            var accuracy = 0.0;
            foreach(var test in testDataset)
            {
                var prediction = model.Predict(test.Input);
                if(prediction.PredictedLabel == test.Output.PercentChangeFromPreviousClose > 0.5m ||
                    !prediction.PredictedLabel == test.Output.PercentChangeFromPreviousClose <= 0.5m)
                {
                    accuracy++;
                }
            }

            accuracy = accuracy / (double)testDataset.Count();
        }

        [Fact]
        public void ShouldTrainUsingWeeklyCandlestickFeatures()
        {
            var datasetService = GetHistoricalFeatureDatasetService(10, 10, FrequencyTimeframe.Weekly);
            var model = new MLStockFastForestCandlestickModel();
            model.Parameters.NumCrossValidations = 3;
            model.Parameters.NumOfTrees = 1000;
            model.Parameters.NumOfLeaves = 200;
            model.Parameters.MinNumOfLeaves = 100;

            var dataset = datasetService.GetAllTrainingData(new DateTime(2000, 1, 1), DateTime.Today, false);
            dataset.Any().Should().BeTrue();

            var trainingResults = model.Train(dataset, 0.0);

            model.Save(_pathToModels);
        }


        private IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetService()
        {
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            var featureExtractor = new CandlestickStockFeatureExtractor();
            //var featureExtractor = new CandlestickStockFeatureExtractorV2();
            //var featureExtractor = new CandlestickSimplifiedStockFeatureExtractor();
            int numberSamples = 40;
            return new CandlestickStockFeatureDatasetService(featureExtractor, stocksRepo, cache, numberSamples);
        }

        private IFeatureDatasetService<FeatureVector> GetHistoricalFeatureDatasetService(int numArticleDays = 10,
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
    }
}
