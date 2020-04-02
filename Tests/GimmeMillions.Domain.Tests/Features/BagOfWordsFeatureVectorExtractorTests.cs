using FluentAssertions;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Features;
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
    public class BagOfWordsFeatureVectorExtractorTests
    {
        private readonly string _pathToArticles = "../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../Repository/Languages";

        [Fact]
        public void ShouldExtractFeatureVectorFromArticles()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var dictionaryRepo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var dictionary = dictionaryRepo.GetFeatureDictionary("FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries");
            dictionary.IsSuccess.Should().BeTrue();

            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var articles = articlesRepo.GetArticles(new DateTime(2000, 5, 27));

            var bow = new BagOfWordsFeatureVectorExtractor(dictionary.Value, textProcessor);

            var featureVector = bow.Extract(articles.Select(x => (x, 1.0f)));
            featureVector.Length.Should().BeGreaterThan(0);
            featureVector.Any(x => x >= 0).Should().BeTrue();

        }
    }
}
