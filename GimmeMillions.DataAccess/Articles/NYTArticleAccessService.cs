using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;
using GimmeMillions.Domain.Keys;
using RestSharp;

namespace GimmeMillions.DataAccess.Articles
{
    public class NYTArticleAccessService : IArticleAccessService
    {
        private readonly IAccessKeyRepository _accessKeyRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly string _nytApiUrl = "https://api.nytimes.com/svc/search/v2/articlesearch.json";
        public NYTArticleAccessService(IAccessKeyRepository accessKeyRepository,
            IArticleRepository articleRepository)
        {
            _accessKeyRepository = accessKeyRepository;
            _articleRepository = articleRepository;

        }
        public Result<IEnumerable<Article>> GetArticles(DateTime dateTime, IEnumerable<FilterQuery> filterQueries)
        {
            var articles = _articleRepository.GetArticles(dateTime).ToList();
            if(!articles.Any())
            {
                articles.AddRange(GetArticlesFromNYTApi(dateTime, filterQueries));
            }

            return Result.Ok<IEnumerable<Article>>(articles);
        }

        private IEnumerable<Article> GetArticlesFromNYTApi(DateTime dateTime, IEnumerable<FilterQuery> filterQueries)
        {
            var keys = _accessKeyRepository.GetKeys().ToList();
            var articles = new List<Article>();
            int currentPage = 0, totalPages = 100;
            foreach(var key in keys)
            {
                while(currentPage < totalPages)
                {
                    string requestUrl = $"{_nytApiUrl}?begin_date={dateTime.ToString("yyyyMMdd")}&end_date={dateTime.ToString("yyyyMMdd")}&page={currentPage}&api-key={key.Secret}";
                    var client = new RestClient(requestUrl);
                    var response = client.Execute<NYTArticleResponse>(new RestRequest());
                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        articles.AddRange(response.Data.Response.Docs);
                        totalPages = response.Data.Response.Meta.Hits;
                        currentPage++;
                    }
                    else
                    {
                        //Something when wrong with the request, probably need to try a different key
                        break;
                    }
                }
            }

            return articles;
        }
    }
}
