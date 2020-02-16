using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Articles
{
    public interface IArticleRepository
    {
        Result AddOrUpdate(Article article);
        IEnumerable<Article> GetArticles(DateTime dateTime);
        bool ContainsArticles(DateTime dateTime);
    }
}
