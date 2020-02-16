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
            var articleId = Guid.NewGuid().ToString();
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
    }
}
