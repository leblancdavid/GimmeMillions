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
        private int _numStockSamples = 10;
        private int _numArticleDays = 10;
        private FrequencyTimeframe _stockSamplingFrequency = FrequencyTimeframe.Daily;
        private bool _includeCompositeIndicators = false;
        private string _articlesEncodingKey;
        private string _stocksEncodingKey;
        private string _encodingKey;

        public bool RefreshCache { get; set; }
        public IStockAccessService StockAccess
        {
            get
            {
                return _stockRepository;
            }
        }


        public HistoricalFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IFeatureExtractor<Article> featureVectorExtractor,
            IArticleAccessService articleRepository,
            IStockAccessService stockRepository,
            int numArticleDays = 10,
            int numStockSamples = 10,
            FrequencyTimeframe stockSamplingFrequency = FrequencyTimeframe.Daily,
            bool includeComposite = false,
            IFeatureCache<FeatureVector> featureCache = null,
            bool refreshCache = false)
        {
            _articleFeatureExtractor = featureVectorExtractor;
            _stockFeatureExtractor = stockFeatureExtractor;
            _articleRepository = articleRepository;
            _stockRepository = stockRepository;

            _numArticleDays = numArticleDays;
            _numStockSamples = numStockSamples;
            _stockSamplingFrequency = stockSamplingFrequency;
            _includeCompositeIndicators = includeComposite;

            _featureCache = featureCache;
            RefreshCache = refreshCache;
            _articlesEncodingKey = $"{_articleFeatureExtractor.Encoding}_{_numArticleDays}d";
            string timeIndicator = _stockSamplingFrequency == FrequencyTimeframe.Daily ? "d" : "w";
            string composite = _includeCompositeIndicators ? "_withComposite" : "";
            _stocksEncodingKey = $"{_stockFeatureExtractor.Encoding}_{_numStockSamples}{timeIndicator}{composite}";
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

            var stocks = _stockRepository.GetStocks(symbol, _stockSamplingFrequency).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            List<StockData> dowStocks = null;
            List<StockData> snpStocks = null;
            List<StockData> nasStocks = null;
            if (_includeCompositeIndicators)
            {
                dowStocks = _stockRepository.GetStocks("^DJI", _stockSamplingFrequency).ToList();
                snpStocks = _stockRepository.GetStocks("^GSPC", _stockSamplingFrequency).ToList();
                nasStocks = _stockRepository.GetStocks("^IXIC", _stockSamplingFrequency).ToList();
            }

            return GetData(symbol, date, stocks, dowStocks, snpStocks, nasStocks);
        }

        private Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date, 
            List<StockData> stocks,
            List<StockData> dowStocks = null,
            List<StockData> snpStocks = null,
            List<StockData> nasStocks = null)
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

            var stocksVector = GetStockFeatureVector(symbol, date, stocks);
            if(stocksVector.IsFailure)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(stocksVector.Error);
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

            if(_includeCompositeIndicators)
            {
                var dowVector = GetStockFeatureVector("^DJI", date, dowStocks);
                if (dowVector.IsFailure)
                {
                    return Result.Failure<(FeatureVector Input, StockData Output)>(dowVector.Error);
                }
                var snpVector = GetStockFeatureVector("^GSPC", date, snpStocks);
                if (snpVector.IsFailure)
                {
                    return Result.Failure<(FeatureVector Input, StockData Output)>(snpVector.Error);
                }
                var nasVector = GetStockFeatureVector("^IXIC", date, nasStocks);
                if (nasVector.IsFailure)
                {
                    return Result.Failure<(FeatureVector Input, StockData Output)>(nasVector.Error);
                }

                var compositeVector = new FeatureVector(
                    articlesVector
                    .Concat(dowVector.Value)
                    .Concat(snpVector.Value)
                    .Concat(nasVector.Value)
                    .Concat(stocksVector.Value).ToArray(), date, _encodingKey);

                if (_featureCache != null)
                    _featureCache.UpdateCache($"{_encodingKey}/{symbol}", compositeVector);

                return Result.Ok((Input: compositeVector, Output: outputStock));
            }

            var extractedVector = new FeatureVector(articlesVector.Concat(stocksVector.Value).ToArray(), date, _encodingKey);

            if (_featureCache != null)
                _featureCache.UpdateCache($"{_encodingKey}/{symbol}", extractedVector);

            return Result.Ok((Input: extractedVector, Output: outputStock));
        }

        private Result<double[]> GetStockFeatureVector(string symbol, DateTime date, List<StockData> stocks)
        {
            var symbolsResult = TryGetStockFeatureFromCache(date, symbol);
            double[] stocksVector;
            if (symbolsResult.IsFailure)
            {
                var stockFeaturesToExtract = new List<(StockData Article, float Weight)>();
                var outputStock = stocks.FirstOrDefault(x => x.Date.Date.Year == date.Year
                                && x.Date.Date.Month == date.Month
                                && x.Date.Date.Day == date.Day);
                int stockIndex;
                if (date > stocks.Last().Date)
                    stockIndex = stocks.Count - 1;
                else
                    stockIndex = stocks.IndexOf(outputStock) - 1;

                for (int i = 0; i < _numStockSamples; ++i)
                {
                    int j = stockIndex - i;
                    if (j < 0)
                    {
                        break;
                    }
                    stockFeaturesToExtract.Add((stocks[j], 1.0f));
                }

                if (stockFeaturesToExtract.Count() != _numStockSamples)
                    return Result.Failure<double[]>(
                        $"No stock data found on {date.ToString("yyyy/MM/dd")}"); ;

                stocksVector = _stockFeatureExtractor.Extract(stockFeaturesToExtract);

                if (_featureCache != null)
                    _featureCache.UpdateCache($"{_stocksEncodingKey}/{symbol}", new FeatureVector(stocksVector, date, _stocksEncodingKey));

            }
            else
            {
                stocksVector = symbolsResult.Value.Data;
            }

            return Result.Ok(stocksVector);
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
                   _stockRepository.UpdateStocks(symbol, _stockSamplingFrequency).ToList() :
                   _stockRepository.GetStocks(symbol, _stockSamplingFrequency).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }

            List<StockData> dowStocks = null;
            List<StockData> snpStocks = null;
            List<StockData> nasStocks = null;
            if (_includeCompositeIndicators)
            {
                dowStocks = updateStocks ?
                   _stockRepository.UpdateStocks("^DJI", _stockSamplingFrequency).ToList() :
                   _stockRepository.GetStocks("^DJI", _stockSamplingFrequency).ToList();
                snpStocks = updateStocks ?
                   _stockRepository.UpdateStocks("^GSPC", _stockSamplingFrequency).ToList() :
                   _stockRepository.GetStocks("^GSPC", _stockSamplingFrequency).ToList();
                nasStocks = updateStocks ?
                   _stockRepository.UpdateStocks("^IXIC", _stockSamplingFrequency).ToList() :
                   _stockRepository.GetStocks("^IXIC", _stockSamplingFrequency).ToList();
            }


            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            //var trainingData = new List<(HistoricalFeatureVector Input, StockData Output)>();
            //foreach (var stock in stocks)
            Parallel.ForEach(stocks, (stock) =>
            {
                if ((startDate == default(DateTime) || startDate < stock.Date) &&
                    (endDate == default(DateTime) || endDate > stock.Date))
                {
                    var data = GetData(symbol, stock.Date, stocks, dowStocks, snpStocks, nasStocks);
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
