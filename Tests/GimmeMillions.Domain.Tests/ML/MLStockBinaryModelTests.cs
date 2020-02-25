using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
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
    public class MLStockBinaryModelTests
    {
        private readonly string _pathToArticles = "../../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../../Repository/Languages";
        private readonly string _pathToStocks = "../../../../../Repository/Stocks";

        [Fact]
        public void ShouldTrainUsingBowFeatures()
        {
            var datasetService = GetTestBoWFeatureDatasetService();
            var model = new MLStockBinaryModel(datasetService, "IWM");

            var trainingResults = model.Train(new DateTime(2010, 1, 1), new DateTime(2012, 6, 1), 0.1);
        }

        private IFeatureDatasetService GetTestBoWFeatureDatasetService()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary("FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries");

            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var stocksRepo = new StockDataRepository(_pathToStocks);
            return new DefaultFeatureDatasetService(bow, articlesRepo, stocksRepo);
        }
    }
}
