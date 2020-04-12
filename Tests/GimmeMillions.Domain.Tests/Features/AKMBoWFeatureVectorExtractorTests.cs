using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Articles;
using GimmeMillions.Domain.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.Domain.Tests.Features
{
    public class AKMBoWFeatureVectorExtractorTests
    {
        private readonly string _pathToArticles = "../../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../../Repository/Languages";
        private readonly string _pathToStocks = "../../../../../Repository/Stocks";
        private readonly string _pathToCache = "../../../../../Repository/Cache";
        private readonly string _pathToModels = "../../../../../Repository/Models";
        private readonly string _pathToKeys = "../../../../../Repository/Keys";

        [Fact]
        public void ShouldBeAbleToTrainAndSave()
        {
            var featureExtractor = GetFeatureExtractor();
            var datasetService = GetBoWFeatureDatasetService();

            var dataset = datasetService.GetTrainingData("F", new DateTime(2000, 1, 10), DateTime.Today);

            var akmFeatureExtractor = new AKMBoWFeatureVectorExtractor(featureExtractor, 1000);
            akmFeatureExtractor.Train(dataset.Value.Select(x => x.Input));

            akmFeatureExtractor.Save(_pathToModels);
            //var akmFeature
        }

        private IFeatureDatasetService<FeatureVector> GetBoWFeatureDatasetService()
        {

            var accessKeys = new NYTApiAccessKeyRepository(_pathToKeys);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articlesAccess = new NYTArticleAccessService(accessKeys, articlesRepo);
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            int numArticlesDays = 10;
            return new DefaultFeatureDatasetService(GetFeatureExtractor(), articlesAccess, stocksRepo, numArticlesDays, cache);
        }

        private IFeatureExtractor<Article> GetFeatureExtractor()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary("USA");

            return new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);
        }
    }
}
