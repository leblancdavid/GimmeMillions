using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Articles
{
    public interface IArticleAccessService
    {
        Result<IEnumerable<Article>> GetArticles(DateTime dateTime, IEnumerable<FilterQuery> filterQueries);
    }
}
