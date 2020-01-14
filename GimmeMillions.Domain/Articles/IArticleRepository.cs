using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Articles
{
    public interface IArticleRepository
    {
        Result AddOrUpdate(Article article);
        Result<IEnumerable<Article>> GetArticles(DateTime dateTime);
        bool ContainsArticles(DateTime dateTime);
    }
}
