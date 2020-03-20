using FluentAssertions;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.DataAccess.Tests.Articles
{
    public class NYTArticleAccessServiceTests
    {

        private readonly string _pathToKeys = "../../../../Repository/Keys";
        private readonly string _pathToArticles = "../../../../Repository/Articles";

        [Fact]
        public void ShouldGetArticlesFromNytApi()
        {
            var keysRepo = new NYTApiAccessKeyRepository(_pathToKeys);
            var articlesRepo = new NYTArticleRepository(_pathToArticles);

            var accessService = new NYTArticleAccessService(keysRepo, articlesRepo);

            var articles = accessService.GetArticles(new DateTime(2012, 5, 27));

            articles.Count().Should().BeGreaterThan(0);

        }
    }
}
