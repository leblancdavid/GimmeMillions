using FluentAssertions;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.Domain.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.Domain.Tests.Features
{
    public class FeaturesDictionaryTests
    {
        private readonly string _pathToArticles = "../../../../Repository/Articles";
        [Fact]
        public void AddArticlesToFeatureDictionary()
        {

            var articlesRepo = new NYTArticleRepository(_pathToArticles);
            var currentDate = new DateTime(2000, 1, 1);
            var endDate = new DateTime(2000, 4, 1);
            var featureDictionary = new FeaturesDictionary();
            while(currentDate <= endDate)
            {
                var articles = articlesRepo.GetArticles(currentDate);
                foreach(var article in articles)
                {
                    featureDictionary.AddArticle(article);
                }
                currentDate = currentDate.AddDays(1.0);
            }

            featureDictionary.Size.Should().BeGreaterThan(0);
            featureDictionary.AverageCount.Should().BeGreaterThan(0.0);
        }

    }
}
