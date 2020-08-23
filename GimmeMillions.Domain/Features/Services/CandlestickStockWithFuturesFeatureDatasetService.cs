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
    public class CandlestickStockWithFuturesFeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IStockAccessService _stockRepository;
        private int _numStockDailySamples = 20;
        private int _stockOutputTimePeriod = 3;
        private string _stocksEncodingKey;
        private string _encodingKey;

        private readonly string DOW_FUTURE_SYMBOL = "DIA";
        private readonly string SNP_FUTURE_SYMBOL = "SPY";
        private readonly string NASDAQ_FUTURE_SYMBOL = "QQQ";
        private readonly string RUSSEL_FUTURE_SYMBOL = "^RUT";

        public bool RefreshCache { get; set; }
        public IStockAccessService StockAccess
        {
            get
            {
                return _stockRepository;
            }
        }


        public CandlestickStockWithFuturesFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IStockAccessService stockRepository,
            int numStockDailySamples = 20,
            int stockOutputLength = 3)
        {
            _stockFeatureExtractor = stockFeatureExtractor;
            _stockRepository = stockRepository;

            _numStockDailySamples = numStockDailySamples;
            _stockOutputTimePeriod = stockOutputLength;

            string timeIndicator = $"{_numStockDailySamples}d-{_stockOutputTimePeriod}p";
            _stocksEncodingKey = $"{_stockFeatureExtractor.Encoding}_{timeIndicator}_withFutures";
            _encodingKey = _stocksEncodingKey;

        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(DateTime startDate = default,
            DateTime endDate = default, bool updateStocks = false)
        {
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            var stockSymbols = _stockRepository.GetSymbols().Where(x => x != DOW_FUTURE_SYMBOL 
                && x != SNP_FUTURE_SYMBOL
                && x != NASDAQ_FUTURE_SYMBOL);

            List<StockData> dowStocks = updateStocks ?
                       _stockRepository.UpdateStocks(DOW_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                       _stockRepository.GetStocks(DOW_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList(); ;
            List<StockData> snpStocks = updateStocks ?
                   _stockRepository.UpdateStocks(SNP_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(SNP_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList(); ;
            List<StockData> nasStocks = updateStocks ?
                   _stockRepository.UpdateStocks(NASDAQ_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(NASDAQ_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> rutStocks = updateStocks ?
                   _stockRepository.UpdateStocks(RUSSEL_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(RUSSEL_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();

            Parallel.ForEach(stockSymbols, symbol =>
            //foreach (var stock in stocks)
            {
                var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(symbol, FrequencyTimeframe.Daily).ToList();
                if (!stocks.Any())
                {
                    return;
                }

                var stockOutputs = _stockRepository.GetStocks(symbol, _stockOutputTimePeriod).ToList();
                if (!stockOutputs.Any())
                {
                    return;
                }

                var td = GetTrainingData(symbol, stocks, stockOutputs,
                    dowStocks, snpStocks, nasStocks, rutStocks,
                    startDate, endDate);
                if (td.IsSuccess)
                {
                    foreach(var sample in td.Value)
                    {
                        trainingData.Add(sample);
                    }
                }
                //}
            });

            return trainingData.OrderBy(x => x.Output.Date);
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {

            var stocks = _stockRepository.GetStocks(symbol, FrequencyTimeframe.Daily).ToList();
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
            List<StockData> dowStocks = _stockRepository.GetStocks(DOW_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> snpStocks = _stockRepository.GetStocks(SNP_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> nasStocks = _stockRepository.GetStocks(NASDAQ_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> rutStocks = _stockRepository.GetStocks(RUSSEL_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();

            var featureVector = GetData(symbol, stockOutput.Date, stocks, dowStocks, snpStocks, nasStocks, rutStocks);
            if(featureVector.IsFailure)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                   featureVector.Error);
            }
            return Result.Ok<(FeatureVector Input, StockData Output)>((featureVector.Value, stockOutput));
        }

        private Result<FeatureVector> GetData(string symbol,
            DateTime date,
            List<StockData> stocks,
            List<StockData> dowStocks = null,
            List<StockData> snpStocks = null,
            List<StockData> nasStocks = null,
            List<StockData> rutStocks = null)
        {
            if (date == null)
            {
                return Result.Failure<FeatureVector>(
                    $"No stock found for symbol '{symbol}'");
            }

            var stocksVector = GetStockFeatureVector(symbol, date, stocks, _numStockDailySamples);
            if (stocksVector.IsFailure)
            {
                return Result.Failure<FeatureVector>(stocksVector.Error);
            }

            var dowVector = GetStockFeatureVector(DOW_FUTURE_SYMBOL, date, dowStocks, _numStockDailySamples);
            if (dowVector.IsFailure)
            {
                return Result.Failure<FeatureVector>(dowVector.Error);
            }
            var snpVector = GetStockFeatureVector(SNP_FUTURE_SYMBOL, date, snpStocks, _numStockDailySamples);
            if (snpVector.IsFailure)
            {
                return Result.Failure<FeatureVector>(snpVector.Error);
            }
            var nasVector = GetStockFeatureVector(NASDAQ_FUTURE_SYMBOL, date, nasStocks, _numStockDailySamples);
            if (nasVector.IsFailure)
            {
                return Result.Failure<FeatureVector>(nasVector.Error);
            }
            var rutVector = GetStockFeatureVector(RUSSEL_FUTURE_SYMBOL, date, rutStocks, _numStockDailySamples);
            if (rutVector.IsFailure)
            {
                return Result.Failure<FeatureVector>(rutVector.Error);
            }

            var compositeVector = new FeatureVector(dowVector.Value
                .Concat(snpVector.Value)
                .Concat(nasVector.Value)
                .Concat(rutVector.Value)
                .Concat(stocksVector.Value).ToArray(), date, _encodingKey);

            return Result.Ok(compositeVector);
        }

        private Result<double[]> GetStockFeatureVector(string symbol, DateTime date, List<StockData> stocks, int numSamples)
        {
            double[] stocksVector;
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

            return Result.Ok(stocksVector);
        }

        public Result<FeatureVector> GetFeatureVector(string symbol, DateTime date)
        {
            var stocks = _stockRepository.GetStocks(symbol, FrequencyTimeframe.Daily).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<FeatureVector>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            List<StockData> dowStocks = _stockRepository.GetStocks(DOW_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> snpStocks = _stockRepository.GetStocks(SNP_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> nasStocks = _stockRepository.GetStocks(NASDAQ_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> rutStocks = _stockRepository.GetStocks(RUSSEL_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();

            return GetData(symbol, date, stocks, dowStocks, snpStocks, nasStocks, rutStocks);
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol, DateTime startDate = default, DateTime endDate = default, bool updateStocks = false)
        {
            var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(symbol, FrequencyTimeframe.Daily).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }

            var stockOutputs = _stockRepository.GetStocks(symbol, _stockOutputTimePeriod).ToList();
            if (!stockOutputs.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No output stocks found for symbol '{symbol}'");
            }

            List<StockData> dowStocks = updateStocks ?
                   _stockRepository.UpdateStocks(DOW_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(DOW_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList(); ;
            List<StockData> snpStocks = updateStocks ?
                   _stockRepository.UpdateStocks(SNP_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(SNP_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList(); ;
            List<StockData> nasStocks = updateStocks ?
                   _stockRepository.UpdateStocks(NASDAQ_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(NASDAQ_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();
            List<StockData> rutStocks = updateStocks ?
                   _stockRepository.UpdateStocks(RUSSEL_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(RUSSEL_FUTURE_SYMBOL, FrequencyTimeframe.Daily).ToList();

            return GetTrainingData(symbol, stocks, stockOutputs, 
                dowStocks, snpStocks, nasStocks, rutStocks,
                startDate, endDate);

        }

        private Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol,
            List<StockData> stocks,
            List<StockData> stockOutputs,
            List<StockData> dowStocks,
            List<StockData> snpStocks, 
            List<StockData> nasStocks,
            List<StockData> rutStocks,
            DateTime startDate = default, DateTime endDate = default)
        {
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            //var trainingData = new List<(HistoricalFeatureVector Input, StockData Output)>();
            //foreach (var stock in stocks)
            Parallel.ForEach(stockOutputs, (stock) =>
            {
                if ((startDate == default(DateTime) || startDate < stock.Date) &&
                    (endDate == default(DateTime) || endDate > stock.Date))
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
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No training data found for symbol '{symbol}' between specified dates");
            }

            return Result.Ok<IEnumerable<(FeatureVector Input, StockData Output)>>(trainingData.OrderBy(x => x.Output.Date));
        }
    }
}
