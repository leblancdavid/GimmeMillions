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
    public class HistoricalFeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        private IFeatureExtractor<Article> _articleFeatureExtractor;
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IArticleAccessService _articleRepository;
        private IStockAccessService _stockRepository;
        private IFeatureCache<FeatureVector> _featureCache;
        private int _numStockDays = 10;
        private int _numArticleDays = 10;
        private string _articlesEncodingKey;
        private string _stocksEncodingKey;
        private string _encodingKey;

        public bool RefreshCache { get; set; }


        public HistoricalFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IFeatureExtractor<Article> featureVectorExtractor,
            IArticleAccessService articleRepository,
            IStockAccessService stockRepository,
            IFeatureCache<FeatureVector> featureCache = null,
            bool refreshCache = false)
        {
            _articleFeatureExtractor = featureVectorExtractor;
            _stockFeatureExtractor = stockFeatureExtractor;
            _articleRepository = articleRepository;
            _stockRepository = stockRepository;
            _featureCache = featureCache;
            RefreshCache = refreshCache;
            _articlesEncodingKey = $"{_articleFeatureExtractor.Encoding}_{_numArticleDays}d";
            _stocksEncodingKey = $"{_stockFeatureExtractor.Encoding}_{_numStockDays}d";
            _encodingKey = $"{_articlesEncodingKey}-{_stocksEncodingKey}";

        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(DateTime startDate = default, 
            DateTime endDate = default, bool updateStocks = false)
        {
            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            var stocks = _stockRepository.GetSymbols();
            foreach (var stock in stocks)
            {
                var td = GetTrainingData(stock, startDate, endDate, updateStocks);
                if (td.IsSuccess)
                {
                    trainingData.AddRange(td.Value);
                }
            }

            return trainingData;
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {
            

            var stocks = _stockRepository.GetStocks(symbol).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            return GetData(symbol, date, stocks);
        }

        private Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date, List<StockData> stocks)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            var outputStock = stocks.FirstOrDefault(x => x.Date.Date.Year == date.Year
                                && x.Date.Date.Month == date.Month
                                && x.Date.Date.Day == date.Day);

            var cacheResult = TryGetFromCache(date, symbol);
            if (cacheResult.IsSuccess)
            {
                return Result.Ok((cacheResult.Value, outputStock));
            }

            var symbolsResult = TryGetStockFeatureFromCache(date, symbol);
            double[] stocksVector;
            if (symbolsResult.IsFailure)
            {
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
                    return Result.Failure<(FeatureVector Input, StockData Output)>(
                        $"No stock data found on {date.ToString("yyyy/MM/dd")}"); ;

                stocksVector = _stockFeatureExtractor.Extract(stockFeaturesToExtract);

                if (_featureCache != null)
                    _featureCache.UpdateCache($"{_stocksEncodingKey}/{symbol}", new FeatureVector(stocksVector, date, _stocksEncodingKey));

            }
            else
            {
                stocksVector = symbolsResult.Value.Data;
            }

            var articleResult = TryGetArticleFromCache(date);
            double[] articlesVector;
            if (articleResult.IsFailure)
            {
                var articlesToExtract = new List<(Article Article, float Weight)>();
                for (int i = 1; i <= _numArticleDays; ++i)
                {
                    articlesToExtract.AddRange(_articleRepository.GetArticles(date.AddDays(-1.0 * i))
                        .Select(x => (x, (float)(_numArticleDays - i + 1) / (float)_numArticleDays)));

                }

                if (!articlesToExtract.Any())
                    return Result.Failure<(FeatureVector Input, StockData Output)>(
                        $"No articles found on {date.ToString("yyyy/MM/dd")}");

                articlesVector = _articleFeatureExtractor.Extract(articlesToExtract);

                if (_featureCache != null)
                    _featureCache.UpdateCache($"{_articlesEncodingKey}", new FeatureVector(articlesVector, date, _articlesEncodingKey));
            }
            else
            {
                articlesVector = articleResult.Value.Data;
            }

            var extractedVector = new FeatureVector(articlesVector.Concat(stocksVector).ToArray(), date, _encodingKey);

            if (_featureCache != null)
                _featureCache.UpdateCache($"{_encodingKey}/{symbol}", extractedVector);

            return Result.Ok((Input: extractedVector, Output: outputStock));
        }

        public Result<FeatureVector> GetFeatureVector(string symbol, DateTime date)
        {
            var output = GetData(symbol, date);
            if(output.IsFailure)
            {
                return Result.Failure<FeatureVector>(output.Error);
            }
            return Result.Ok(output.Value.Input);
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol, DateTime startDate = default, DateTime endDate = default, bool updateStocks = false)
        {
            var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol).ToList() :
                   _stockRepository.GetStocks(symbol).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }


            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            //var trainingData = new List<(HistoricalFeatureVector Input, StockData Output)>();
            //foreach (var stock in stocks)
            Parallel.ForEach(stocks, (stock) =>
            {
                if ((startDate == default(DateTime) || startDate < stock.Date) &&
                    (endDate == default(DateTime) || endDate > stock.Date))
                {
                    var data = GetData(symbol, stock.Date, stocks);
                    if(data.IsFailure)
                    {
                        return;
                    }

                    trainingData.Add(data.Value);
                }
            });
            //}
            if (!trainingData.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No training data found for symbol '{symbol}' between specified dates");
            }

            return Result.Ok<IEnumerable<(FeatureVector Input, StockData Output)>>(trainingData.OrderBy(x => x.Output.Date));
        }

        Result<FeatureVector> TryGetFromCache(DateTime date, string symbol)
        {
            if (_featureCache == null)
            {
                return Result.Failure<FeatureVector>($"No feature cache provided for date {date.ToString("mm/dd/yyyy")}");
            }
            if (RefreshCache)
            {
                return Result.Failure<FeatureVector>($"RefreshCache is on, therefore features will be re-computed");
            }

            return _featureCache.GetFeature($"{_encodingKey}/{symbol}", date);
        }

        Result<FeatureVector> TryGetStockFeatureFromCache(DateTime date, string symbol)
        {
            if (_featureCache == null)
            {
                return Result.Failure<FeatureVector>($"No feature cache provided for date {date.ToString("mm/dd/yyyy")}");
            }
            if (RefreshCache)
            {
                return Result.Failure<FeatureVector>($"RefreshCache is on, therefore features will be re-computed");
            }

            return _featureCache.GetFeature($"{_stocksEncodingKey}/{symbol}", date);
        }

        Result<FeatureVector> TryGetArticleFromCache(DateTime date)
        {
            if (_featureCache == null)
            {
                return Result.Failure<FeatureVector>($"No feature cache provided for date {date.ToString("mm/dd/yyyy")}");
            }
            if (RefreshCache)
            {
                return Result.Failure<FeatureVector>($"RefreshCache is on, therefore features will be re-computed");
            }

            return _featureCache.GetFeature($"{_articlesEncodingKey}", date);
        }
    }
}
