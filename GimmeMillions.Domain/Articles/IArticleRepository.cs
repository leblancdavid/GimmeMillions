using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Articles
{
    public interface IArticleRepository
    {
        Result AddOrUpdate(Article article);
        IEnumerable<Article> GetArticles(DateTime dateTime);
        IEnumerable<Article> GetArticles();
        bool ContainsArticles(DateTime dateTime);
        bool ArticleExists(string id);
        Result<Article> GetArticle(string id);
    }
}
