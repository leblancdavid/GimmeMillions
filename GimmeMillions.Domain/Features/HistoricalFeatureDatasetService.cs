using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class HistoricalFeatureDatasetService : IFeatureDatasetService<HistoricalFeatureVector>
    {
        private IFeatureExtractor<Article> _articleFeatureExtractor;
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IArticleAccessService _articleRepository;
        private IStockAccessService _stockRepository;
        private IFeatureCache<HistoricalFeatureVector> _featureCache;
        private int _numStockDays = 10;
        private int _numArticleDays = 10;
        private string _encodingKey;
        
        public bool RefreshCache { get; set; }


        public HistoricalFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IFeatureExtractor<Article> featureVectorExtractor,
            IArticleAccessService articleRepository,
            IStockAccessService stockRepository,
            IFeatureCache<HistoricalFeatureVector> featureCache = null,
            bool refreshCache = false)
        {
            _articleFeatureExtractor = featureVectorExtractor;
            _stockFeatureExtractor = stockFeatureExtractor;
            _articleRepository = articleRepository;
            _stockRepository = stockRepository;
            _featureCache = featureCache;
            RefreshCache = refreshCache;
            _encodingKey = $"{_articleFeatureExtractor.Encoding}_{_numArticleDays}d-{_stockFeatureExtractor.Encoding}_{_numStockDays}d";

        }

        public IEnumerable<(HistoricalFeatureVector Input, StockData Output)> GetAllTrainingData(DateTime startDate = default, 
            DateTime endDate = default)
        {
            var trainingData = new List<(HistoricalFeatureVector Input, StockData Output)>();
            var stocks = _stockRepository.GetSymbols();
            foreach (var stock in stocks)
            {
                var td = GetTrainingData(stock, startDate, endDate);
                if (td.IsSuccess)
                {
                    trainingData.AddRange(td.Value);
                }
            }

            return trainingData;


            
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
            var cacheResult = TryGetFromCache(date, symbol);
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
                date, _encodingKey);

            if (_featureCache != null)
                _featureCache.UpdateCache($"{_encodingKey}/{symbol}", extractedVector);

            return Result.Ok((Input: extractedVector, Output: outputStock));
        }

        public Result<HistoricalFeatureVector> GetFeatureVector(string symbol, DateTime date)
        {
            var cacheResult = TryGetFromCache(date, symbol);
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
                return Result.Failure<HistoricalFeatureVector>(
                    $"No articles found on {date.ToString("yyyy/MM/dd")}"); ;

            var stocks = _stockRepository.GetStocks(symbol).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<HistoricalFeatureVector>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }
            var outputStock = stocks.FirstOrDefault(x => x.Date.Date.Year == date.Year
                                && x.Date.Date.Month == date.Month
                                && x.Date.Date.Day == date.Day);

            var stockFeaturesToExtract = new List<(StockData Article, float Weight)>();
            int stockIndex = stocks.IndexOf(outputStock);
            for (int i = 1; i <= _numStockDays; ++i)
            {
                int j = stockIndex - i;
                if (j < 0)
                {
                    break;
                }
                stockFeaturesToExtract.Add((stocks[j], 1.0f));
            }

            if (stockFeaturesToExtract.Count() != _numStockDays)
                return Result.Failure<HistoricalFeatureVector>(
                    $"No stock data found on {date.ToString("yyyy/MM/dd")}"); ;

            
            var extractedVector = new HistoricalFeatureVector(
                _articleFeatureExtractor.Extract(articlesToExtract),
                _stockFeatureExtractor.Extract(stockFeaturesToExtract),
                date, _encodingKey);

            if (_featureCache != null)
                _featureCache.UpdateCache($"{_encodingKey}/{symbol}", extractedVector);

            return Result.Ok(extractedVector);
        }

        public Result<IEnumerable<(HistoricalFeatureVector Input, StockData Output)>> GetTrainingData(string symbol, DateTime startDate = default, DateTime endDate = default)
        {
            var stocks = _stockRepository.UpdateStocks(symbol).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<IEnumerable<(HistoricalFeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }


            var trainingData = new ConcurrentBag<(HistoricalFeatureVector Input, StockData Output)>();
            //var trainingData = new List<(HistoricalFeatureVector Input, StockData Output)>();
            //foreach (var stock in stocks)
            Parallel.ForEach(stocks, (stock) =>
            {
                if ((startDate == default(DateTime) || startDate < stock.Date) &&
                    (endDate == default(DateTime) || endDate > stock.Date))
                {
                    var cacheResult = TryGetFromCache(stock.Date, symbol);
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


                        var stockFeaturesToExtract = new List<(StockData Article, float Weight)>();
                        int stockIndex = stocks.IndexOf(stock);
                        for (int i = 1; i <= _numStockDays; ++i)
                        {
                            int j = stockIndex - i;
                            if (j < 0)
                            {
                                break;
                            }
                            stockFeaturesToExtract.Add((stocks[j], 1.0f));
                        }

                        if (stockFeaturesToExtract.Count() != _numStockDays)
                            return;

                        var extractedVector = new HistoricalFeatureVector(
                            _articleFeatureExtractor.Extract(articlesToExtract),
                            _stockFeatureExtractor.Extract(stockFeaturesToExtract),
                            stock.Date, _encodingKey);

                        if (_featureCache != null)
                            _featureCache.UpdateCache($"{_encodingKey}/{symbol}", extractedVector);

                        trainingData.Add((Input: extractedVector, Output: stock));
                    }

                }
            });
            //}
            if (!trainingData.Any())
            {
                return Result.Failure<IEnumerable<(HistoricalFeatureVector Input, StockData Output)>>(
                    $"No training data found for symbol '{symbol}' between specified dates");
            }

            return Result.Ok<IEnumerable<(HistoricalFeatureVector Input, StockData Output)>>(trainingData.OrderBy(x => x.Output.Date));
        }

        Result<HistoricalFeatureVector> TryGetFromCache(DateTime date, string symbol)
        {
            if (_featureCache == null)
            {
                return Result.Failure<HistoricalFeatureVector>($"No feature cache provided for date {date.ToString("mm/dd/yyyy")}");
            }
            if (RefreshCache)
            {
                return Result.Failure<HistoricalFeatureVector>($"RefreshCache is on, therefore features will be re-computed");
            }

            return _featureCache.GetFeature($"{_encodingKey}/{symbol}", date);
        }
    }
}
