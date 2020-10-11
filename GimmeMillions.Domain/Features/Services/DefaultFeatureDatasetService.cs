using System;
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
    public class DefaultFeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        private IFeatureExtractor<Article> _articleFeatureExtractor;
        private IArticleAccessService _articleRepository;
        private IStockAccessService _stockRepository;
        private IFeatureCache<FeatureVector> _featureCache;
        private int _numArticleDays = 10;
        private string _encodingKey;

        public bool RefreshCache { get; set; }

        public IStockAccessService StockAccess => _stockRepository;

        public DefaultFeatureDatasetService(IFeatureExtractor<Article> featureVectorExtractor,
            IArticleAccessService articleRepository,
            IStockAccessService stockRepository,
            int numArticleDays = 10,
            IFeatureCache<FeatureVector> featureCache = null,
            bool refreshCache = false)
        {
            _articleFeatureExtractor = featureVectorExtractor;
            _articleRepository = articleRepository;
            _stockRepository = stockRepository;
            _numArticleDays = numArticleDays;
            _featureCache = featureCache;
            RefreshCache = refreshCache; 
            _encodingKey = $"{_articleFeatureExtractor.Encoding}_{_numArticleDays}d";

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

            var articlesToExtract = new List<(Article Article, float Weight)>();
            for (int i = 1; i <= _numArticleDays; ++i)
            {
                articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-1.0 * i))
                    .Select(x => (x, (float)(_numArticleDays - i + 1) / (float)_numArticleDays)));

            }
            if (!articlesToExtract.Any())
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No articles found on {date.ToString("yyyy/MM/dd")}"); ;

            var extractedVector = new FeatureVector(_articleFeatureExtractor.Extract(articlesToExtract),
                date, _encodingKey);

            if (_featureCache != null)
                _featureCache.UpdateCache(_encodingKey, extractedVector);

            return Result.Ok((Input: extractedVector, Output: stock));

        }

        public Result<FeatureVector> GetFeatureVector(string symbol, DateTime date)
        {
            var cacheResult = TryGetFromCache(date);
            if (cacheResult.IsSuccess)
            {
                return cacheResult;
            }
            var articlesToExtract = new List<(Article Article, float Weight)>();
            for (int i = 1; i <= _numArticleDays; ++i)
            {
                articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-1.0 * i))
                    .Select(x => (x, (float)(_numArticleDays - i + 1) / (float)_numArticleDays)));

            }
            if (!articlesToExtract.Any())
                return Result.Failure< FeatureVector> (
                    $"No articles found on {date.ToString("yyyy/MM/dd")}"); ;

            var extractedVector = new FeatureVector(_articleFeatureExtractor.Extract(articlesToExtract),
                date, _encodingKey);

            if (_featureCache != null)
                _featureCache.UpdateCache(_encodingKey, extractedVector);

            return Result.Ok(extractedVector);
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol,
            IDatasetFilter filter = null,
            bool updateStocks = false)
        {
            var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol).ToList() :
                   _stockRepository.GetStocks(symbol).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }

            if(filter == null)
            {
                filter = new DefaultDatasetFilter();
            }
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            Parallel.ForEach(stocks, (stock) =>
            {
                if (filter.Pass(stock))
                {
                    var cacheResult = TryGetFromCache(stock.Date);
                    if (cacheResult.IsSuccess)
                    {
                        trainingData.Add((Input: cacheResult.Value, Output: stock));
                    }
                    else
                    {
                        var articlesToExtract = new List<(Article Article, float Weight)>();
                        for (int i = 1; i <= _numArticleDays; ++i)
                        {
                            articlesToExtract.AddRange(_articleRepository.GetArticles(stock.Date.AddDays(-1.0 * i))
                                .Select(x => (x, (float)(_numArticleDays - i + 1) / (float)_numArticleDays)));

                        }
                        if (!articlesToExtract.Any())
                            return;

                        var extractedVector = new FeatureVector(_articleFeatureExtractor.Extract(articlesToExtract),
                            stock.Date, _encodingKey);

                        if (_featureCache != null)
                            _featureCache.UpdateCache(_encodingKey, extractedVector);

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

            return _featureCache.GetFeature(_encodingKey, date);
        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(
            IDatasetFilter filter = null,
             bool updateStocks = false)
        {
            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            var stocks = _stockRepository.GetSymbols();
            foreach(var stock in stocks)
            {
                var td = GetTrainingData(stock, filter, updateStocks);
                if(td.IsSuccess)
                {
                    trainingData.AddRange(td.Value);
                }
            }

            return trainingData;
        }
    }
}
