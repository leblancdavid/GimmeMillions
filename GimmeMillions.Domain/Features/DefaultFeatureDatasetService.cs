using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;
using GimmeMillions.Domain.Stocks;

namespace GimmeMillions.Domain.Features
{
    public class DefaultFeatureDatasetService : IFeatureDatasetService
    {
        private IFeatureVectorExtractor _featureVectorExtractor;
        private IArticleRepository _articleRepository;
        private IStockRepository _stockRepository;
        public DefaultFeatureDatasetService(IFeatureVectorExtractor featureVectorExtractor,
            IArticleRepository articleRepository,
            IStockRepository stockRepository)
        {
            _featureVectorExtractor = featureVectorExtractor;
            _articleRepository = articleRepository;
            _stockRepository = stockRepository;
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {
            var stock = _stockRepository.GetStocks(symbol).FirstOrDefault(x => x.Date.Date == date.Date);
            if(stock == null)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            var articleDate = stock.Date.AddDays(-1.0);
            var articles = _articleRepository.GetArticles(articleDate);
            if (!articles.Any())
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No articles found on {articleDate.ToString("yyyy/MM/dd")}"); ;

            return Result.Ok((Input: _featureVectorExtractor.Extract(articles.Select(x => (x, 1.0))), Output: stock));

        }

        public Result<FeatureVector> GetData(DateTime date)
        {
            var articleDate = date.Date.AddDays(-1.0);
            var articles = _articleRepository.GetArticles(articleDate);
            if (!articles.Any())
                return Result.Failure<FeatureVector>(
                    $"No articles found on {articleDate.ToString("yyyy/MM/dd")}"); ;

            return Result.Ok(_featureVectorExtractor.Extract(articles.Select(x => (x, 1.0))));
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol,
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime))
        {
            var stocks = _stockRepository.GetStocks(symbol);
            if(!stocks.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            foreach(var stock in stocks)
            {
                if((startDate == default(DateTime) || startDate < stock.Date) &&
                    (endDate == default(DateTime) || endDate > stock.Date))
                {
                    var articles = _articleRepository.GetArticles(stock.Date.AddDays(-1.0));
                    if (!articles.Any())
                        continue;
                    
                    trainingData.Add((Input : _featureVectorExtractor.Extract(articles.Select(x => (x, 1.0))), Output: stock));
                }
            }

            if(!trainingData.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No training data found for symbol '{symbol}' between specified dates");
            }

            return Result.Ok<IEnumerable<(FeatureVector Input, StockData Output)>>(trainingData);
        }
    }
}
