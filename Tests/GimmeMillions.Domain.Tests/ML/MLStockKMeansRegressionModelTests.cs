﻿using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.Domain.Tests.ML
{
    public class MLStockKMeansRegressionModelTests
    {
        private readonly string _pathToArticles = "../../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../../Repository/Languages";
        private readonly string _pathToStocks = "../../../../../Repository/Stocks";
        private readonly string _pathToKeys = "../../../../Repository/Keys";

        [Fact]
        public void ShouldTrainUsingRandomFeatures()
        {
            var datasetService = GetTestBoWFeatureDatasetService();
            //var datasetService = GetTestRandomDatasetService(442, 16);
            var model = new MLStockKMeansRegressionModel(datasetService, "IWM");

            var trainingResults = model.Train(new DateTime(2010, 1, 1), new DateTime(2012, 1, 1), 0.1);
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


            return new DefaultFeatureDatasetService(bow, articlesAccess, stocksRepo);
        }

        private IFeatureDatasetService<FeatureVector> GetTestRandomDatasetService(int seed, int featureSize)
        {
            return new RandomFeatureDatasetService(seed, featureSize);
        }
    }
}
