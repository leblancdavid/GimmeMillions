using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
        private readonly int _callInterval_ms = 2000;
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
                var articlesGetResult = GetArticlesFromNYTApi(dateTime, filterQueries);
                if(articlesGetResult.IsFailure)
                {
                    return articlesGetResult;
                }

                articles.AddRange(articlesGetResult.Value);
                foreach(var article in articles)
                {
                    _articleRepository.AddOrUpdate(article);
                }
            }

            return Result.Ok<IEnumerable<Article>>(articles);
        }

        private Result<IEnumerable<Article>> GetArticlesFromNYTApi(DateTime dateTime, IEnumerable<FilterQuery> filterQueries)
        {
            var keys = _accessKeyRepository.GetKeys().ToList();
            var articles = new List<Article>();
            int currentPage = 0, totalArticles = int.MaxValue;
            bool articlesRetrieved = true;
            while (articles.Count < totalArticles && articlesRetrieved)
            {
                articlesRetrieved = false;
                foreach (var key in keys)
                {
                    Thread.Sleep(_callInterval_ms);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    string requestUrl = $"{_nytApiUrl}?begin_date={dateTime.ToString("yyyyMMdd")}&end_date={dateTime.ToString("yyyyMMdd")}&page={currentPage}&api-key={key.Key}";
                    var client = new RestClient(requestUrl);
                    var response = client.Execute<NYTArticleResponse>(new RestRequest());
                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        foreach(var article in response.Data.Response.Docs)
                        {
                            //Just make sure we update the current date
                            article.Date = dateTime;
                        }

                        articles.AddRange(response.Data.Response.Docs);
                        totalArticles = response.Data.Response.Meta.Hits;
                        currentPage++;
                        articlesRetrieved = true;
                        if(articles.Count >= totalArticles)
                        {
                            break;
                        }
                    } //otherwise we'll try a different key
                }
            }

            if (!articlesRetrieved)
            {
                return Result.Failure<IEnumerable<Article>>($"Unable to retrieve all the articles for {dateTime.ToString("yyyyMMdd")}");
            }

            return Result.Ok<IEnumerable<Article>>(articles);
        }
    }
}
