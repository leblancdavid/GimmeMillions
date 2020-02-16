using FluentAssertions;
using GimmeMillions.DataAccess.Articles;
using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.DataAccess.Tests.Articles
{
    public class NYTArticleRepositoryTests
    {
        private readonly string _pathToArticles = "../../../../Repository/Articles";

        [Fact]
        public void ShouldAddNewArticles()
        {
            Directory.Exists(_pathToArticles).Should().BeTrue();
            var repo = new NYTArticleRepository(_pathToArticles);
            var articleId = "2f93df14-3b90-40e8-90d3-109134178f59";
            var articleDate = new DateTime(1, 1, 1);
            var result = repo.AddOrUpdate(new Article()
            {
                Date = articleDate,
                Uri = articleId,
                LeadParagraph = "This is just a test article"
            }); ; ;
            result.IsFailure.Should().BeFalse();

            File.Exists($"{_pathToArticles}/{articleDate.ToString("yyyy/MM/dd")}/{articleId}.json").Should().BeTrue();
        }

        [Fact]
        public void ShouldGetArticlesForSpecificDate()
        {
            Directory.Exists(_pathToArticles).Should().BeTrue();
            var repo = new NYTArticleRepository(_pathToArticles);
            var articleDate = new DateTime(1, 1, 1);

            var articles = repo.GetArticles(articleDate);

            articles.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public void ShouldGetAllExistingArticles()
        {
            Directory.Exists(_pathToArticles).Should().BeTrue();
            var repo = new NYTArticleRepository(_pathToArticles);

            var articles = repo.GetArticles();

            articles.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public void ShouldCheckIfThereAreArticlesForSpecificDate()
        {
            Directory.Exists(_pathToArticles).Should().BeTrue();
            var repo = new NYTArticleRepository(_pathToArticles);
            var articleDate = new DateTime(1, 1, 1);

            var articlesCheck = repo.ContainsArticles(articleDate);

            articlesCheck.Should().BeTrue();
        }

        [Fact]
        public void ShouldCheckIfAnArticleExists()
        {
            Directory.Exists(_pathToArticles).Should().BeTrue();
            var repo = new NYTArticleRepository(_pathToArticles);

            var articlesCheck = repo.ArticleExists("2f93df14-3b90-40e8-90d3-109134178f59");

            articlesCheck.Should().BeTrue();

            articlesCheck = repo.ArticleExists("some article id that doesn't exist");

            articlesCheck.Should().BeFalse();
        }

        [Fact]
        public void ShouldGetASpecificArticleById()
        {
            Directory.Exists(_pathToArticles).Should().BeTrue();
            var repo = new NYTArticleRepository(_pathToArticles);

            var article = repo.GetArticle("2f93df14-3b90-40e8-90d3-109134178f59");

            article.IsSuccess.Should().BeTrue();

            article = repo.GetArticle("some article id that doesn't exist");

            article.IsSuccess.Should().BeFalse();
        }
    }
}
