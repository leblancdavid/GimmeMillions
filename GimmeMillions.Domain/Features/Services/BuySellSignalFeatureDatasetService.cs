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
        private double[] _gaussianKernel;
        private double[] _gaussianDerivative;
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


        public BuySellSignalFeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IStockAccessService stockRepository,
            int numStockDailySamples = 20,
            int derivativeKernel = 9)
        {
            _stockFeatureExtractor = stockFeatureExtractor;
            _stockRepository = stockRepository;

            _numStockDailySamples = numStockDailySamples;
            _derivativeKernel = derivativeKernel;
            _gaussianKernel = GetGaussianKernel(_derivativeKernel);
            _gaussianDerivative = GetGaussianDerivativeKernel(_derivativeKernel);
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

            var stocksVector = GetStockFeatureVector(date, stocks, _numStockDailySamples);
            if (stocksVector.IsFailure)
            {
                return Result.Failure<FeatureVector>(stocksVector.Error);
            }

            var compositeVector = new FeatureVector(stocksVector.Value, date, _encodingKey);
            return Result.Ok(compositeVector);
        }

        private Result<double[]> GetStockFeatureVector(DateTime date, List<StockData> stocks, int numSamples)
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

            var smooth = new double[stocks.Count];
            int k = _derivativeKernel / 2;
            for (int i = 0; i < stocks.Count - _derivativeKernel; ++i)
            {
                for (int j = 0; j < _derivativeKernel; ++j)
                {
                    smooth[k] += _gaussianKernel[j] * (double)stocks[i+j].Close;
                }
                ++k;
            }

            var derivatives = new double[stocks.Count];
            k = _derivativeKernel / 2;
            for (int i = 0; i < stocks.Count - _derivativeKernel; ++i)
            {
                for(int j = 0; j < _derivativeKernel; ++j)
                {
                    derivatives[k] += _gaussianDerivative[j] * smooth[i+j];
                }
                ++k;
            }
            var inflectionIndex = new List<int>();
            for (int i = 1; i < derivatives.Length - 1; ++i)
            {
                //if the derivative goes from negative to positive, or positive to negative, there's a shift in the trend
                if ((derivatives[i] > 0.0 && derivatives[i - 1] <= 0.0) ||
                    (derivatives[i] < 0.0 && derivatives[i - 1] >= 0.0))
                {
                    inflectionIndex.Add(i);
                }
            }

            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            for (int i = 1; i < inflectionIndex.Count; ++i)
            {
                var data = GetData(symbol, stocks[inflectionIndex[i - 1]].Date, stocks);
                if (data.IsFailure)
                    continue;

                var start = stocks[inflectionIndex[i - 1]];
                var end = stocks[inflectionIndex[i]];
                decimal high = 0.0m;
                decimal low = decimal.MaxValue;
                decimal volume = 0.0m;
                for (int j = inflectionIndex[i - 1]; j < inflectionIndex[i]; ++j)
                {
                    if(stocks[j].Low < low)
                    {
                        low = stocks[j].Low;
                    }
                    if (stocks[j].High > high)
                    {
                        high = stocks[j].High;
                    }
                    volume += stocks[j].Volume;
                }

                var output = new StockData(symbol, start.Date, start.Open, high, low, end.Close, end.AdjustedClose, start.PreviousClose);
                trainingData.Add((data.Value, output));
            }

            if (!trainingData.Any())
            {
                return Result.Failure<IEnumerable<(FeatureVector Input, StockData Output)>>(
                    $"No training data found for symbol '{symbol}' between specified dates");
            }

            return Result.Ok<IEnumerable<(FeatureVector Input, StockData Output)>>(trainingData.OrderBy(x => x.Output.Date));
        }

        private double[] GetGaussianDerivativeKernel(int length)
        {
            double sigma = (double)length / 6.0;
            double e = 2.71828;
            double pi = 3.14159;
            double f = -1.0;// / (Math.Sqrt(2.0 * pi) * Math.Pow(sigma, 2.0));
            int center = length / 2;
            var kernel = new double[length];
            for (int i = 0; i < length; ++i)
            {
                double x = i - center;
                kernel[i] = f * x * Math.Pow(e, -1.0 * x * x / (2.0 * sigma * sigma));
            }

            return kernel;
        }

        private double[] GetGaussianKernel(int length)
        {
            double sigma = (double)length / 6.0;
            double e = 2.71828;
            double pi = 3.14159;
            double f = 1.0 / Math.Sqrt(sigma * sigma * 2.0 * pi);
            int center = length / 2;
            var kernel = new double[length];
            for (int i = 0; i < length; ++i)
            {
                double x = i - center;
                kernel[i] = f * Math.Pow(e, -1.0 * x * x / (2.0 * sigma * sigma));
            }

            return kernel;
        }

        public IEnumerable<FeatureVector> GetFeatures(string symbol)
        {
            var stocks = _stockRepository.GetStocks(symbol, FrequencyTimeframe.Daily).ToList();
            var features = new List<FeatureVector>();
            if (!stocks.Any())
            {
                return features;
            }

            foreach(var date in stocks)
            {
                var f = GetStockFeatureVector(date.Date, stocks, _numStockDailySamples);
                if (f.IsFailure)
                    continue;

                features.Add(new FeatureVector(f.Value, date.Date, _encodingKey));
            }

            return features;
        }
    }
}
