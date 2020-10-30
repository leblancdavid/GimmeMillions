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
    public class BuySellSignalFeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IStockAccessService _stockRepository;
        private int _numStockDailySamples = 20;
        private int _derivativeKernel = 9;
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


        public CandlestickStockWithFuturesFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IStockAccessService stockRepository,
            int numStockDailySamples = 20,
            int derivativeKernel = 9)
        {
            _stockFeatureExtractor = stockFeatureExtractor;
            _stockRepository = stockRepository;

            _numStockDailySamples = numStockDailySamples;
            _derivativeKernel = derivativeKernel;

            string timeIndicator = $"{_numStockDailySamples}d-{_derivativeKernel}k";
            _stocksEncodingKey = $"{_stockFeatureExtractor.Encoding}_{timeIndicator}";
            _encodingKey = _stocksEncodingKey;

        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(
            IDatasetFilter filter = null, 
            bool updateStocks = false)
        {
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            var stockSymbols = _stockRepository.GetSymbols();

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

                var td = GetTrainingData(symbol, stocks, filter);
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
            var stockOutput = _stockRepository.GetStocks(symbol).FirstOrDefault(
                                x => x.Date.Date.Year == date.Year
                                && x.Date.Date.Month == date.Month
                                && x.Date.Date.Day == date.Day);
            if (stockOutput == null)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            var featureVector = GetData(symbol, stockOutput.Date, stocks);
            if(featureVector.IsFailure)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                   featureVector.Error);
            }
            return Result.Ok<(FeatureVector Input, StockData Output)>((featureVector.Value, stockOutput));
        }

        private Result<FeatureVector> GetData(string symbol,
            DateTime date,
            List<StockData> stocks)
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

            var compositeVector = new FeatureVector(stocksVector.Value, date, _encodingKey);
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

            return GetData(symbol, date, stocks);
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(
            string symbol,
            IDatasetFilter filter = null, 
            bool updateStocks = false)
        {
            var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol, FrequencyTimeframe.Daily).ToList() :
                   _stockRepository.GetStocks(symbol, FrequencyTimeframe.Daily).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No stocks found for symbol '{symbol}'");
            }

         
            return GetTrainingData(symbol, stocks, filter);

        }

        private Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol,
            List<StockData> stocks,
            IDatasetFilter filter = null)
        {
            if (filter == null)
            {
                filter = new DefaultDatasetFilter();
            }
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            //var trainingData = new List<(FeatureVector Input, StockData Output)>();

            //decimal averageDayRange = stockOutputs.Average(x => x.PercentDayRange);
            ////foreach (var stock in stockOutputs)
            //Parallel.ForEach(stockOutputs, (stock) =>
            //{
            //    if (filter.Pass(stock))
            //    {
            //        var data = GetData(symbol, stock.Date, stocks);
            //        if (data.IsFailure)
            //        {
            //            //continue;
            //            return;
            //        }

            //        stock.AveragePercentDayRange = averageDayRange;
            //        trainingData.Add((data.Value, stock));
            //    }
            //});
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
