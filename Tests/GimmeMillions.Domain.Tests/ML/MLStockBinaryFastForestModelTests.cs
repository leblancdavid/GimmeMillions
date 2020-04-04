using FluentAssertions;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.ML.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.Domain.Tests.ML
{
    public class MLStockBinaryFastForestModelTests
    {
        private readonly string _pathToArticles = "../../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../../Repository/Languages";
        private readonly string _pathToStocks = "../../../../../Repository/Stocks";
        private readonly string _pathToCache = "../../../../../Repository/Cache";
        private readonly string _pathToModels = "../../../../../Repository/Models";
        private readonly string _pathToKeys = "../../../../Repository/Keys";

        [Fact]
        public void ShouldTrainUsingBowFeatures()
        {
            var datasetService = GetTestBoWFeatureDatasetService();
            //var datasetService = GetTestRandomDatasetService(422, 200);
            var model = new MLStockBinaryFastForestModel();
            model.Parameters.PcaRank = 128;
            model.Parameters.FeatureSelectionRank = model.Parameters.PcaRank * 10;
            model.Parameters.NumIterations = 3;
            model.Parameters.NumCrossValidations = 10;
            model.Parameters.NumOfTrees = 512;
            model.Parameters.NumOfLeaves = 16;
            model.Parameters.MinNumOfLeaves = 5;

            var dataset = datasetService.GetTrainingData("AMZN", new DateTime(2000, 1, 1), new DateTime(2004, 1, 1));
            dataset.IsSuccess.Should().BeTrue();

            var trainingResults = model.Train(dataset.Value, 0.1);
        }

        [Fact]
        public void ShouldTrainUsingRandomFeatures()
        {
            //var datasetService = GetTestBoWFeatureDatasetService();
            var datasetService = GetTestRandomDatasetService(422, 200);
            var model = new MLStockBinaryFastForestModel();
            model.Parameters.PcaRank = 10;
            model.Parameters.FeatureSelectionRank = model.Parameters.PcaRank * 10;
            model.Parameters.NumIterations = 1;
            model.Parameters.NumCrossValidations = 10;
            model.Parameters.NumOfTrees = 20;
            model.Parameters.NumOfLeaves = 4;
            model.Parameters.MinNumOfLeaves = 5;

            var dataset = datasetService.GetTrainingData("RNG", new DateTime(2010, 1, 1), new DateTime(2018, 8, 1));
            dataset.IsSuccess.Should().BeTrue();

            var trainingResults = model.Train(dataset.Value, 0.1);
        }

        [Fact]
        public void ShouldBeAbleToTrainAndPredictRandomFeatures()
        {
            var datasetService = GetTestRandomDatasetService(422, 200);
            var model = new MLStockBinaryFastForestModel();
            model.Parameters.PcaRank = 10;
            model.Parameters.FeatureSelectionRank = model.Parameters.PcaRank * 10;
            model.Parameters.NumIterations = 1;
            model.Parameters.NumCrossValidations = 10;
            model.Parameters.NumOfTrees = 20;
            model.Parameters.NumOfLeaves = 4;
            model.Parameters.MinNumOfLeaves = 5;

            var dataset = datasetService.GetTrainingData("RNG", new DateTime(2010, 1, 1), new DateTime(2018, 8, 1));
            dataset.IsSuccess.Should().BeTrue();

            var trainingResults = model.Train(dataset.Value, 0.1);

            var textExample = datasetService.GetData("RNG", new DateTime(1, 1, 1));
            textExample.IsSuccess.Should().BeTrue();

            var prediction = model.Predict(textExample.Value.Input);
        }

        [Fact]
        public void ShouldBeAbleToSaveARandomModel()
        {
            var datasetService = GetTestRandomDatasetService(422, 200);
            var model = new MLStockBinaryFastForestModel();
            model.Parameters.PcaRank = 10;
            model.Parameters.FeatureSelectionRank = model.Parameters.PcaRank * 10;
            model.Parameters.NumIterations = 1;
            model.Parameters.NumCrossValidations = 10;
            model.Parameters.NumOfTrees = 20;
            model.Parameters.NumOfLeaves = 4;
            model.Parameters.MinNumOfLeaves = 5;

            var dataset = datasetService.GetTrainingData("RandomTestModel", new DateTime(2010, 1, 1), new DateTime(2018, 8, 1));
            dataset.IsSuccess.Should().BeTrue();

            var trainingResults = model.Train(dataset.Value, 0.1);

            var saveResult = model.Save(_pathToModels);
            saveResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ShouldBeAbleToLoadAnExistingModel()
        {
            var datasetService = GetTestRandomDatasetService(422, 200);
            var model = new MLStockBinaryFastForestModel();
            model.Parameters.PcaRank = 10;
            model.Parameters.FeatureSelectionRank = model.Parameters.PcaRank * 10;
            model.Parameters.NumIterations = 1;
            model.Parameters.NumCrossValidations = 10;
            model.Parameters.NumOfTrees = 20;
            model.Parameters.NumOfLeaves = 4;
            model.Parameters.MinNumOfLeaves = 5;

            string symbol = "RandomTestModel";
            var dataset = datasetService.GetTrainingData(symbol, new DateTime(2010, 1, 1), new DateTime(2018, 8, 1));
            dataset.IsSuccess.Should().BeTrue();

            var trainingResults = model.Train(dataset.Value, 0.1);

            var testExample = datasetService.GetData(symbol, new DateTime(1, 1, 1));
            testExample.IsSuccess.Should().BeTrue();
            var preLoadPrediction = model.Predict(testExample.Value.Input);
            

            var saveResult = model.Save(_pathToModels);
            saveResult.IsSuccess.Should().BeTrue();

            var loadResult = model.Load(_pathToModels, symbol, testExample.Value.Input.Encoding);
            loadResult.IsSuccess.Should().BeTrue();

            var postLoadPrediction = model.Predict(testExample.Value.Input);

            preLoadPrediction.PredictedLabel.Should().Be(postLoadPrediction.PredictedLabel);
            preLoadPrediction.Score.Should().Be(postLoadPrediction.Score);
        }

        private IFeatureDatasetService<FeatureVector> GetTestBoWFeatureDatasetService()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary("FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries");

            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesAccess = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);

            return new DefaultFeatureDatasetService(bow, articlesAccess, stocksRepo, cache);
        }

        private IFeatureDatasetService<FeatureVector> GetTestRandomDatasetService(int seed, int featureSize)
        {
            return new RandomFeatureDatasetService(seed, featureSize);
        }
    }
}
