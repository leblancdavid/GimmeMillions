using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickStockFeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IStockAccessService _stockRepository;
        private IFeatureCache<FeatureVector> _featureCache;
        private int _numStockDailySamples = 20;
        private StockDataPeriod _stockOutputTimePeriod;
        private bool _includeCompositeIndicators = false;
        private string _stocksEncodingKey;
        private string _encodingKey;

        public bool RefreshCache { get; set; }
        public StockDataPeriod Period { get; private set; }
        public IStockAccessService StockAccess
        {
            get
            {
                return _stockRepository;
            }
        }


        public CandlestickStockFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IStockAccessService stockRepository,
            StockDataPeriod stockOutputLength,
            int numStockDailySamples = 20,
            bool includeComposite = false,
            IFeatureCache<FeatureVector> featureCache = null,
            bool refreshCache = false)
        {
            _stockFeatureExtractor = stockFeatureExtractor;
            _stockRepository = stockRepository;

            _numStockDailySamples = numStockDailySamples;
            _stockOutputTimePeriod = stockOutputLength;
            _includeCompositeIndicators = includeComposite;

            _featureCache = featureCache;
            RefreshCache = refreshCache;
            string timeIndicator = $"{_numStockDailySamples}d-{_stockOutputTimePeriod}p";
            string composite = _includeCompositeIndicators ? "_withComposite" : "";
            _stocksEncodingKey = $"{_stockFeatureExtractor.Encoding}_{timeIndicator}{composite}";
            _encodingKey = _stocksEncodingKey;

        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(IStockFilter filter = null,
            bool updateStocks = false, int historyLimit = 0)
        {
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            var stocks = _stockRepository.GetSymbols().Where(x => x != "^DJI" && x != "^GSPC" && x != "^IXIC");
            Parallel.ForEach(stocks, stock =>
            //foreach (var stock in stocks)
            {
                var td = GetTrainingData(stock, filter, updateStocks);
                foreach (var sample in td)
                {
                    trainingData.Add(sample);
                }
                //}
            });

            return trainingData.OrderBy(x => x.Output.Date);
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date, int historyLimit = 0)
        {

            var stocks = _stockRepository.GetStocks(symbol, _stockOutputTimePeriod).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }
            var stockOutput = _stockRepository.GetStocks(symbol, _stockOutputTimePeriod).FirstOrDefault(
                                x => x.Date.Date.Year == date.Year
                                && x.Date.Date.Month == date.Month
                                && x.Date.Date.Day == date.Day);
            if (stockOutput == null)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }
            List<StockData> dowStocks = null;
            List<StockData> snpStocks = null;
            List<StockData> nasStocks = null;
            if (_includeCompositeIndicators)
            {
                dowStocks = _stockRepository.GetStocks("^DJI", StockDataPeriod.Day).ToList();
                snpStocks = _stockRepository.GetStocks("^GSPC", StockDataPeriod.Day).ToList();
                nasStocks = _stockRepository.GetStocks("^IXIC", StockDataPeriod.Day).ToList();
            }

            var featureVector = GetData(symbol, stockOutput.Date, stocks, dowStocks, snpStocks, nasStocks);
            if (featureVector.IsFailure)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                   featureVector.Error);
            }
            return Result.Success<(FeatureVector Input, StockData Output)>((featureVector.Value, stockOutput));
        }

        private Result<FeatureVector> GetData(string symbol,
            DateTime date,
            List<StockData> stocks,
            List<StockData> dowStocks = null,
            List<StockData> snpStocks = null,
            List<StockData> nasStocks = null)
        {
            if (date == null)
            {
                return Result.Failure<FeatureVector>(
                    $"No stock found for symbol '{symbol}'");
            }

            var cacheResult = TryGetFromCache(date, symbol);
            if (cacheResult.IsSuccess)
            {
                return cacheResult;
            }

            var stocksVector = GetStockFeatureVector(symbol, date, stocks, _numStockDailySamples);
            if (stocksVector.IsFailure)
            {
                return Result.Failure<FeatureVector>(stocksVector.Error);
            }

            if (_includeCompositeIndicators)
            {
                var dowVector = GetStockFeatureVector("^DJI", date, dowStocks, _numStockDailySamples);
                if (dowVector.IsFailure)
                {
                    return Result.Failure<FeatureVector>(dowVector.Error);
                }
                var snpVector = GetStockFeatureVector("^GSPC", date, snpStocks, _numStockDailySamples);
                if (snpVector.IsFailure)
                {
                    return Result.Failure<FeatureVector>(snpVector.Error);
                }
                var nasVector = GetStockFeatureVector("^IXIC", date, nasStocks, _numStockDailySamples);
                if (nasVector.IsFailure)
                {
                    return Result.Failure<FeatureVector>(nasVector.Error);
                }

                var compositeVector = new FeatureVector(dowVector.Value
                    .Concat(snpVector.Value)
                    .Concat(nasVector.Value)
                    .Concat(stocksVector.Value).ToArray(), date, _encodingKey);

                if (_featureCache != null)
                    _featureCache.UpdateCache($"{_encodingKey}/{symbol}", compositeVector);

                return Result.Success(compositeVector);
            }

            var extractedVector = new FeatureVector(stocksVector.Value.ToArray(), date, _encodingKey);

            if (_featureCache != null)
                _featureCache.UpdateCache($"{_encodingKey}/{symbol}", extractedVector);

            return Result.Success(extractedVector);
        }

        private Result<double[]> GetStockFeatureVector(string symbol, DateTime date, List<StockData> stocks, int numSamples)
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

                for (int i = 0; i < numSamples; ++i)
                {
                    int j = stockIndex - i;
                    if (j < 0)
                    {
                        break;
                    }
                    stockFeaturesToExtract.Add((stocks[j], 1.0f));
                }

                if (stockFeaturesToExtract.Count() != numSamples)
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

            return Result.Success(stocksVector);
        }

        public Result<FeatureVector> GetFeatureVector(string symbol, DateTime date, int historyLimit = 0)
        {
            var stocks = _stockRepository.GetStocks(symbol, StockDataPeriod.Day).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<FeatureVector>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            List<StockData> dowStocks = null;
            List<StockData> snpStocks = null;
            List<StockData> nasStocks = null;
            if (_includeCompositeIndicators)
            {
                dowStocks = _stockRepository.GetStocks("^DJI", StockDataPeriod.Day).ToList();
                snpStocks = _stockRepository.GetStocks("^GSPC", StockDataPeriod.Day).ToList();
                nasStocks = _stockRepository.GetStocks("^IXIC", StockDataPeriod.Day).ToList();
            }

            return GetData(symbol, date, stocks, dowStocks, snpStocks, nasStocks);
        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetTrainingData(
            string symbol, IStockFilter filter = null,
            bool updateStocks = false, int historyLimit = 0)
        {
            var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol, StockDataPeriod.Day).ToList() :
                   _stockRepository.GetStocks(symbol, StockDataPeriod.Day).ToList();
            if (!stocks.Any())
            {
                return new List<(FeatureVector Input, StockData Output)>();
            }

            var stockOutputs = _stockRepository.GetStocks(symbol, _stockOutputTimePeriod).ToList();
            if (!stockOutputs.Any())
            {
                return new List<(FeatureVector Input, StockData Output)>();
            }

            List<StockData> dowStocks = null;
            List<StockData> snpStocks = null;
            List<StockData> nasStocks = null;
            if (_includeCompositeIndicators)
            {
                dowStocks = updateStocks ?
                   _stockRepository.UpdateStocks("^DJI", StockDataPeriod.Day).ToList() :
                   _stockRepository.GetStocks("^DJI", StockDataPeriod.Day).ToList();
                snpStocks = updateStocks ?
                   _stockRepository.UpdateStocks("^GSPC", StockDataPeriod.Day).ToList() :
                   _stockRepository.GetStocks("^GSPC", StockDataPeriod.Day).ToList();
                nasStocks = updateStocks ?
                   _stockRepository.UpdateStocks("^IXIC", StockDataPeriod.Day).ToList() :
                   _stockRepository.GetStocks("^IXIC", StockDataPeriod.Day).ToList();
            }

            if (filter == null)
            {
                filter = new DefaultStockFilter();
            }
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            //var trainingData = new List<(HistoricalFeatureVector Input, StockData Output)>();
            //foreach (var stock in stocks)
            Parallel.ForEach(stockOutputs, (stock) =>
            {
                if (filter.Pass(stock))
                {
                    var data = GetData(symbol, stock.Date, stocks, dowStocks, snpStocks, nasStocks);
                    if (data.IsFailure)
                    {
                        //continue;
                        return;
                    }

                    trainingData.Add((data.Value, stock));
                }
            });
            //}
            if (!trainingData.Any())
            {
                return new List<(FeatureVector Input, StockData Output)>(); ;
            }

            return trainingData.OrderBy(x => x.Output.Date);
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

        public IEnumerable<FeatureVector> GetFeatures(string symbol)
        {
            throw new NotImplementedException();
        }

        public Result<FeatureVector> GetFeatureVector(string symbol, out StockData last, int historyLimit = 0)
        {
            throw new NotImplementedException();
        }
    }
}
