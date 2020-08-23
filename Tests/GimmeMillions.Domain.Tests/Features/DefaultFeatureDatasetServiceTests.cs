using FluentAssertions;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Articles;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.Domain.Tests.Features
{
    public class DefaultFeatureDatasetServiceTests
    {
        private readonly string _pathToArticles = "../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../Repository/Languages";
        private readonly string _pathToStocks = "../../../../Repository/Stocks";
        private readonly string _pathToKeys = "../../../../Repository/Keys";

        [Fact]
        public void ShouldGetTrainingData()
        {
            var bow = GetTestBoWFeatureExtractor();
            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesService = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), new PlaceholderStockHistoryRepository(), _pathToStocks);

            var featureDatasetService = new DefaultFeatureDatasetService(bow, articlesService, stocksRepo);
            var trainingData = featureDatasetService.GetTrainingData("IWM");

            trainingData.IsSuccess.Should().BeTrue();
            trainingData.Value.Count().Should().BeGreaterThan(0);
        }

        private IFeatureExtractor<Article> GetTestBoWFeatureExtractor()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary("FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries");
            dictionary.IsSuccess.Should().BeTrue();

            return new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
        }
    }
}
