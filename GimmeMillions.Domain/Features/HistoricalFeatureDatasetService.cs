using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class HistoricalFeatureDatasetService : IFeatureDatasetService<HistoricalFeatureVector>
    {
        private IFeatureExtractor<Article> _articleFeatureExtractor;
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IArticleAccessService _articleRepository;
        private IStockAccessService _stockRepository;
        private IFeatureCache _featureCache;
        private int _numStockDays = 5;
        private int _numArticleDays = 10;
        
        public bool RefreshCache { get; set; }


        public HistoricalFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IFeatureExtractor<Article> featureVectorExtractor,
            IArticleAccessService articleRepository,
            IStockAccessService stockRepository,
            IFeatureCache featureCache = null,
            bool refreshCache = false)
        {
            _articleFeatureExtractor = featureVectorExtractor;
            _stockFeatureExtractor = stockFeatureExtractor;
            _articleRepository = articleRepository;
            _stockRepository = stockRepository;
            _featureCache = featureCache;
            RefreshCache = refreshCache;

        }

        public IEnumerable<(HistoricalFeatureVector Input, StockData Output)> GetAllTrainingData(DateTime startDate = default, DateTime endDate = default)
        {
            throw new NotImplementedException();
        }

        public Result<(HistoricalFeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {
            var stocks = _stockRepository.GetStocks(symbol).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<(HistoricalFeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            var outputStock = stocks.FirstOrDefault(x => x.Date.Date.Year == date.Year
                                && x.Date.Date.Month == date.Month
                                && x.Date.Date.Day == date.Day);
            var cacheResult = TryGetFromCache(date);
            if (cacheResult.IsSuccess)
            {
                return Result.Ok((Input: cacheResult.Value, Output: outputStock));
            }

            var articlesToExtract = new List<(Article Article, float Weight)>();
            for(int i = 1; i <= _numArticleDays; ++i)
            {
                articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-1.0 * i))
                    .Select(x => (x, (float)(_numArticleDays - i + 1) / (float)_numArticleDays)));

            }
            
            if (!articlesToExtract.Any())
                return Result.Failure<(HistoricalFeatureVector Input, StockData Output)>(
                    $"No articles found on {date.ToString("yyyy/MM/dd")}"); ;


            var stockFeaturesToExtract = new List<(StockData Article, float Weight)>();
            int stockIndex = stocks.IndexOf(outputStock);
            for (int i = 1; i <= _numStockDays; ++i)
            {
                int j = stockIndex - i;
                if(j < 0)
                {
                    break;
                }
                stockFeaturesToExtract.Add((stocks[j], 1.0f));
            }

            if (stockFeaturesToExtract.Count() != _numStockDays)
                return Result.Failure<(HistoricalFeatureVector Input, StockData Output)>(
                    $"No stock data found on {date.ToString("yyyy/MM/dd")}"); ;

            var extractedVector = new HistoricalFeatureVector(
                _articleFeatureExtractor.Extract(articlesToExtract),
                _stockFeatureExtractor.Extract(stockFeaturesToExtract),
                date, _articleFeatureExtractor.Encoding);

            if (_featureCache != null)
                _featureCache.UpdateCache(extractedVector);

            return Result.Ok((Input: extractedVector, Output: outputStock));
        }

        public Result<HistoricalFeatureVector> GetFeatureVector(string symbol, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Result<IEnumerable<(HistoricalFeatureVector Input, StockData Output)>> GetTrainingData(string symbol, DateTime startDate = default, DateTime endDate = default)
        {
            throw new NotImplementedException();
        }

        Result<HistoricalFeatureVector> TryGetFromCache(DateTime date)
        {
            if (_featureCache == null)
            {
                return Result.Failure<HistoricalFeatureVector>($"No feature cache provided for date {date.ToString("mm/dd/yyyy")}");
            }
            if (RefreshCache)
            {
                return Result.Failure<HistoricalFeatureVector>($"RefreshCache is on, therefore features will be re-computed");
            }

            return _featureCache.GetFeature<HistoricalFeatureVector>(_articleFeatureExtractor.Encoding, date);
        }
    }
}
