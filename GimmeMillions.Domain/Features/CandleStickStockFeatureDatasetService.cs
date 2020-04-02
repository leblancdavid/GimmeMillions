using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickStockFeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        public bool RefreshCache { get; set; }

        private IFeatureExtractor<StockData> _featureVectorExtractor;
        private IStockAccessService _stockRepository;
        private IFeatureCache _featureCache;

        public CandlestickStockFeatureDatasetService(IFeatureExtractor<StockData> featureVectorExtractor,
            IStockAccessService stockRepository,
            IFeatureCache featureCache = null,
            bool refreshCache = false)
        {
            _featureVectorExtractor = featureVectorExtractor;
            _stockRepository = stockRepository;
            _featureCache = featureCache;
            RefreshCache = refreshCache;
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {
            var stocks = _stockRepository.GetStocks(symbol);
            var stock = stocks.FirstOrDefault(x => x.Date.Date == date.Date);
            if (stock == null)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            var cacheResult = TryGetFromCache(stock.Date);
            if (cacheResult.IsSuccess)
            {
                return Result.Ok((Input: cacheResult.Value, Output: stock));
            }

            var currentDate = stock.Date.AddDays(-1.0);
            var stocksToExtract = new List<(StockData Data, float Weight)>();
            int sampleDays = 5;
            for(int i = 0; i < sampleDays; ++i)
            {
                while(currentDate.DayOfWeek == DayOfWeek.Sunday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    currentDate = currentDate.AddDays(-1.0);
                }
                var s = stocks.FirstOrDefault(x => x.Date.Date.Year == currentDate.Year
                                && x.Date.Date.Month == currentDate.Month
                                && x.Date.Date.Day == currentDate.Day);
                if(s != null)
                {
                    stocksToExtract.Add((s, 1.0f));
                }

                currentDate = currentDate.AddDays(-1.0);
            }

            if (stocksToExtract.Count != sampleDays)
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stocks found on {date.Date.ToString("yyyy/MM/dd")}"); ;

            var extractedVector = new FeatureVector(_featureVectorExtractor.Extract(stocksToExtract), stock.Date, _featureVectorExtractor.Encoding);
            
            if (_featureCache != null)
                _featureCache.UpdateCache(extractedVector);

            return Result.Ok((Input: extractedVector, Output: stock));

        }

        public Result<FeatureVector> GetFeatureVector(string symbol, DateTime date)
        {
            var cacheResult = TryGetFromCache(date);
            if (cacheResult.IsSuccess)
            {
                return cacheResult;
            }

            var stocks = _stockRepository.GetStocks(symbol);
            var stock = stocks.FirstOrDefault(x => x.Date.Date == date.Date);

            var currentDate = stock.Date.AddDays(-1.0);
            var stocksToExtract = new List<(StockData Data, float Weight)>();
            int sampleDays = 5;
            for (int i = 0; i < sampleDays; ++i)
            {
                while (currentDate.DayOfWeek == DayOfWeek.Sunday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    currentDate = currentDate.AddDays(-1.0);
                }
                var s = stocks.FirstOrDefault(x => x.Date.Date.Year == currentDate.Year
                                && x.Date.Date.Month == currentDate.Month
                                && x.Date.Date.Day == currentDate.Day);
                if (s != null)
                {
                    stocksToExtract.Add((s, 1.0f));
                }

                currentDate = currentDate.AddDays(-1.0);
            }

            if (stocksToExtract.Count != sampleDays)
                return Result.Failure<FeatureVector>(
                    $"No stocks found on {date.Date.ToString("yyyy/MM/dd")}"); ;

            var extractedVector = new FeatureVector(_featureVectorExtractor.Extract(stocksToExtract), stock.Date, _featureVectorExtractor.Encoding);

            if (_featureCache != null)
                _featureCache.UpdateCache(extractedVector);

            return Result.Ok(extractedVector);
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol,
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime))
        {
            var stocks = _stockRepository.UpdateStocks(symbol);
            if (!stocks.Any())
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
                        var currentDate = stock.Date.AddDays(-1.0);
                        var stocksToExtract = new List<(StockData Data, float Weight)>();
                        int sampleDays = 5;
                        for (int i = 0; i < sampleDays; ++i)
                        {
                            while (currentDate.DayOfWeek == DayOfWeek.Sunday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                            {
                                currentDate = currentDate.AddDays(-1.0);
                            }
                            var s = stocks.FirstOrDefault(x => x.Date.Date.Year == currentDate.Year 
                                && x.Date.Date.Month == currentDate.Month
                                && x.Date.Date.Day == currentDate.Day);
                            if (s != null)
                            {
                                stocksToExtract.Add((s, 1.0f));
                            }

                            currentDate = currentDate.AddDays(-1.0);
                        }

                        if (stocksToExtract.Count != sampleDays)
                            return ;

                        var extractedVector = new FeatureVector(_featureVectorExtractor.Extract(stocksToExtract), stock.Date, _featureVectorExtractor.Encoding);

                        if (_featureCache != null)
                            _featureCache.UpdateCache(extractedVector);

                        trainingData.Add((Input: extractedVector, Output: stock));
                    }

                }
            });

            if (!trainingData.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No training data found for symbol '{symbol}' between specified dates");
            }

            return Result.Ok<IEnumerable<(FeatureVector Input, StockData Output)>>(trainingData.OrderBy(x => x.Output.Date));
        }

        Result<FeatureVector> TryGetFromCache(DateTime date)
        {
            if (_featureCache == null)
            {
                return Result.Failure<FeatureVector>($"No feature cache provided for date {date.ToString("mm/dd/yyyy")}");
            }
            if (RefreshCache)
            {
                return Result.Failure<FeatureVector>($"RefreshCache is on, therefore features will be re-computed");
            }

            return _featureCache.GetFeature<FeatureVector>(_featureVectorExtractor.Encoding, date);
        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(DateTime startDate = default, DateTime endDate = default)
        {
            var trainingData = new List<(FeatureVector Input, StockData Output)>();
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
    }
}
