using FluentAssertions;
using GimmeMillions.DataAccess.Articles;
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
    public class FeaturesDictionaryTests
    {
        private readonly string _pathToArticles = "../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../Repository/Languages";
        [Fact]
        public void AddArticlesToFeatureDictionary()
        {

            var featureDictionary = CreateTestFeatureDictionary("FeaturesDictionaryTests.AddArticlesToFeatureDictionary", _pathToArticles, _pathToLanguage);
            featureDictionary.Size.Should().BeGreaterThan(0);
        }

        public static FeaturesDictionary CreateTestFeatureDictionary(string dictionaryId, string pathToArticles, string pathToLanguage)
        {
            var articlesRepo = new NYTArticleRepository(pathToArticles);
            var currentDate = new DateTime(2000, 1, 1);
            var endDate = new DateTime(2000, 4, 1);
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{pathToLanguage}/usa.txt"));
            var textProcessor = new DefaultTextProcessor(featureChecker);

            var featureDictionary = new FeaturesDictionary();
            featureDictionary.DictionaryId = dictionaryId;
            while (currentDate <= endDate)
            {
                var articles = articlesRepo.GetArticles(currentDate);
                foreach (var article in articles)
                {
                    featureDictionary.AddArticle(article, textProcessor);
                }
                currentDate = currentDate.AddDays(1.0);
            }

            return featureDictionary;
        }

    }
}
