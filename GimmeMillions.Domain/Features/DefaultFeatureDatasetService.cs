﻿using System;
using System.Collections.Concurrent;
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
        private IStockAccessService _stockRepository;
        private IFeatureCache _featureCache;

        public bool RefreshCache { get; set; }
        public DefaultFeatureDatasetService(IFeatureVectorExtractor featureVectorExtractor,
            IArticleRepository articleRepository,
            IStockAccessService stockRepository,
            IFeatureCache featureCache = null,
            bool refreshCache = false)
        {
            _featureVectorExtractor = featureVectorExtractor;
            _articleRepository = articleRepository;
            _stockRepository = stockRepository;
            _featureCache = featureCache;
            RefreshCache = refreshCache;
            
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {
            var stock = _stockRepository.GetStocks(symbol).FirstOrDefault(x => x.Date.Date == date.Date);
            if(stock == null)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            var cacheResult = TryGetFromCache(stock.Date);
            if(cacheResult.IsSuccess)
            {
                return Result.Ok((Input: cacheResult.Value, Output: stock));
            }

            var articleDate = stock.Date.AddDays(-1.0);
            var articlesToExtract = new List<(Article Article, float Weight)>();
            articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-1.0)).Select(x => (x, 1.0f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-2.0)).Select(x => (x, 0.8f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-3.0)).Select(x => (x, 0.6f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-4.0)).Select(x => (x, 0.4f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-5.0)).Select(x => (x, 0.2f)));

            if (!articlesToExtract.Any())
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No articles found on {articleDate.ToString("yyyy/MM/dd")}"); ;

            var extractedVector = _featureVectorExtractor.Extract(articlesToExtract);
            extractedVector.Date = stock.Date;

            if (_featureCache != null)
                _featureCache.UpdateCache(extractedVector);

            return Result.Ok((Input: extractedVector, Output: stock));

        }

        public Result<FeatureVector> GetData(DateTime date)
        {
            var cacheResult = TryGetFromCache(date);
            if (cacheResult.IsSuccess)
            {
                return cacheResult;
            }

            var articleDate = date.AddDays(-1.0);
            var articlesToExtract = new List<(Article Article, float Weight)>();
            articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-1.0)).Select(x => (x, 1.0f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-2.0)).Select(x => (x, 0.8f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-3.0)).Select(x => (x, 0.6f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-4.0)).Select(x => (x, 0.4f)));
            articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-5.0)).Select(x => (x, 0.2f)));
            if (!articlesToExtract.Any())
                return Result.Failure<FeatureVector>(
                    $"No articles found on {articleDate.ToString("yyyy/MM/dd")}"); ;

            var extractedVector = _featureVectorExtractor.Extract(articlesToExtract);
            extractedVector.Date = date;

            if (_featureCache != null)
                _featureCache.UpdateCache(extractedVector);

            return Result.Ok(extractedVector);
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol,
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime))
        {
            var stocks = _stockRepository.UpdateStocks(symbol, startDate, endDate);
            if(!stocks.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }

            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            Parallel.ForEach(stocks, (stock) =>
            {
                if ((startDate == default(DateTime) || startDate < stock.Date) &&
                    (endDate == default(DateTime) || endDate > stock.Date))
                {
                    var cacheResult = TryGetFromCache(stock.Date);
                    if (cacheResult.IsSuccess)
                    {
                        trainingData.Add((Input: cacheResult.Value, Output: stock));
                    }
                    else
                    {
                        var articlesToExtract = new List<(Article Article, float Weight)>();

                        articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-1.0)).Select(x => (x, 1.0f)));
                        articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-2.0)).Select(x => (x, 0.8f)));
                        articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-3.0)).Select(x => (x, 0.6f)));
                        articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-4.0)).Select(x => (x, 0.4f)));
                        articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-5.0)).Select(x => (x, 0.2f)));
                        if (!articlesToExtract.Any())
                            return;

                        var extractedVector = _featureVectorExtractor.Extract(articlesToExtract);
                        extractedVector.Date = stock.Date;

                        if (_featureCache != null)
                            _featureCache.UpdateCache(extractedVector);

                        trainingData.Add((Input: extractedVector, Output: stock));
                    }
                    
                }
            });

            if(!trainingData.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No training data found for symbol '{symbol}' between specified dates");
            }

            return Result.Ok<IEnumerable<(FeatureVector Input, StockData Output)>>(trainingData.OrderBy(x => x.Output.Date));
        }

        Result<FeatureVector> TryGetFromCache(DateTime date)
        {
            if(_featureCache == null)
            {
                return Result.Failure<FeatureVector>($"No feature cache provided for date {date.ToString("mm/dd/yyyy")}");
            }
            if(RefreshCache)
            {
                return Result.Failure<FeatureVector>($"RefreshCache is on, therefore features will be re-computed");
            }

            return _featureCache.GetFeature(_featureVectorExtractor.Encoding, date);
        }

    }
}
