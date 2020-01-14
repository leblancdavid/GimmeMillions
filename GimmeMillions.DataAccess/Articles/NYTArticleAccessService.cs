using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;

namespace GimmeMillions.DataAccess.Articles
{
    public class NYTArticleAccessService : IArticleAccessService
    {

        public Result<IEnumerable<Article>> GetArticles(DateTime dateTime)
        {
            var articles = new List<Article>();
            return Result.Ok<IEnumerable<Article>>(articles);
        }
    }
}
