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
    public class BuySellSignalFeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IStockAccessService _stockRepository;
        private int _numStockDailySamples = 20;
        private int _derivativeKernel = 9;
        private int _signalOffset = 0;
        private int _predictionLength = 5;
        private double[] _gaussianKernel;
        private double[] _gaussianDerivative;
        private string _stocksEncodingKey;
        private string _encodingKey;

        public StockDataPeriod Period { get; private set; }

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
            StockDataPeriod period,
            int numStockDailySamples = 20,
            int derivativeKernel = 9,
            int signalOffset = 0,
            int predictionLength = 10)
        {
            _stockFeatureExtractor = stockFeatureExtractor;
            _stockRepository = stockRepository;

            _numStockDailySamples = numStockDailySamples;
            _derivativeKernel = derivativeKernel;
            _signalOffset = signalOffset;
            _predictionLength = predictionLength;
            _gaussianKernel = GetGaussianKernel(_derivativeKernel);
            _gaussianDerivative = GetGaussianDerivativeKernel(_derivativeKernel);
            string timeIndicator = $"{_numStockDailySamples}d-{_derivativeKernel}k";
            _stocksEncodingKey = $"{_stockFeatureExtractor.Encoding}_{timeIndicator}";
            _encodingKey = _stocksEncodingKey;
            Period = period;

        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(
            IStockFilter filter = null,
            bool updateStocks = false, int historyLimit = 0,
            bool addMirroredSamples = false)
        {
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            var stockSymbols = _stockRepository.GetSymbols();

            Console.WriteLine("Retrieving stocks training samples...");
            var updateLock = new object();
            //Parallel.ForEach(stockSymbols, symbol =>
            foreach (var symbol in stockSymbols)
            {
                List<StockData> stocks = null;
                stocks = updateStocks ?
                      _stockRepository.UpdateStocks(symbol, Period, historyLimit).ToList() :
                      _stockRepository.GetStocks(symbol, Period, historyLimit).ToList();

                Console.WriteLine($"{symbol}: {stocks.Count} found");
                if (!stocks.Any())
                {
                    continue;
                }
                var td = GetTrainingData(symbol, stocks, filter).ToList();
                if (addMirroredSamples)
                {
                    foreach (var data in stocks)
                    {
                        data.ApplyScaling(-1.0m);
                    }
                    td.AddRange(GetTrainingData(symbol, stocks, filter));
                }

                foreach (var sample in td)
                {
                    trainingData.Add(sample);
                }

                //}
            }

            Console.WriteLine($"Done retrieving training data. {trainingData.Count} total samples collected.");
            return trainingData.OrderBy(x => x.Output.Date);
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date, int historyLimit = 0)
        {

            var stocks = _stockRepository.GetStocks(symbol, Period).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }
            var stockOutput = _stockRepository.GetStocks(symbol, Period).FirstOrDefault(
                                x => x.Date.Date.Year == date.Year
                                && x.Date.Date.Month == date.Month
                                && x.Date.Date.Day == date.Day);
            if (stockOutput == null)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            var featureVector = GetData(symbol, stockOutput.Date, stocks);
            if (featureVector.IsFailure)
            {
                return Result.Failure<(FeatureVector Input, StockData Output)>(
                   featureVector.Error);
            }
            return Result.Success<(FeatureVector Input, StockData Output)>((featureVector.Value, stockOutput));
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

            var compositeVector = new FeatureVector(stocksVector.Value, stocks.Last().Date, _encodingKey);
            return Result.Success(compositeVector);
        }

        private Result<double[]> GetStockFeatureVector(DateTime date, List<StockData> stocks, int numSamples)
        {
            try
            {
                double[] stocksVector;
                var stockFeaturesToExtract = new List<(StockData Article, float Weight)>();
                //var outputStock = stocks.FirstOrDefault(x => x.Date.Date.Year == date.Year
                //                && x.Date.Date.Month == date.Month
                //                && x.Date.Date.Day == date.Day); 
                var outputStock = stocks.FirstOrDefault(x => x.Date.Date == date.Date);
                int stockIndex = 0;
                if (outputStock == null || date > stocks.Last().Date)
                {
                    for (int i = stocks.Count - 1; i >= 0; --i)
                    {
                        if (stocks[i].Date < date)
                        {
                            stockIndex = i;
                            break;
                        }

                    }
                }
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

                return Result.Success(stocksVector);
            }
            catch (Exception ex)
            {
                return Result.Failure<double[]>(ex.Message);
            }
        }

        public Result<FeatureVector> GetFeatureVector(string symbol, DateTime date, int historyLimit = 0)
        {
            var stocks = _stockRepository.GetStocks(symbol, Period, historyLimit).ToList();
            if (!stocks.Any())
            {
                return Result.Failure<FeatureVector>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            return GetData(symbol, date, stocks);
        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetTrainingData(
            string symbol,
            IStockFilter filter = null,
            bool updateStocks = false, int historyLimit = 0,
            bool addMirroredSamples = false)
        {
            var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol, Period, historyLimit).ToList() :
                   _stockRepository.GetStocks(symbol, Period, historyLimit).ToList();
            if (!stocks.Any())
            {
                return new List<(FeatureVector Input, StockData Output)>();
            }

            var trainingData = GetTrainingData(symbol, stocks, filter).ToList();
            if(addMirroredSamples)
            {
                foreach(var data in stocks)
                {
                    data.ApplyScaling(-1.0m);
                }
                trainingData.AddRange(GetTrainingData(symbol, stocks, filter));
            }
            return trainingData;

        }

        private IEnumerable<(FeatureVector Input, StockData Output)> GetTrainingData(string symbol,
            List<StockData> stocks,
            IStockFilter filter = null)
        {
            try
            {
                if (filter == null)
                {
                    filter = new DefaultStockFilter();
                }

                var smooth = new double[stocks.Count];
                int k = _derivativeKernel / 2;
                for (int i = 0; i < stocks.Count - _derivativeKernel; ++i)
                {
                    for (int j = 0; j < _derivativeKernel; ++j)
                    {
                        smooth[k] += _gaussianKernel[j] * (double)stocks[i + j].Close;
                    }
                    ++k;
                }

                var derivatives = new double[stocks.Count];
                k = _derivativeKernel / 2;
                for (int i = 0; i < stocks.Count - _derivativeKernel; ++i)
                {
                    for (int j = 0; j < _derivativeKernel; ++j)
                    {
                        derivatives[k] += _gaussianDerivative[j] * smooth[i + j];
                    }
                    ++k;
                }
                var inflectionIndex = new List<(int Index, bool BuySignal)>();
                for (int i = 0; i < derivatives.Length; ++i)
                {
                    if ((derivatives[i] > 0.0 && derivatives[i - 1] <= 0.0))
                    {
                        inflectionIndex.Add((i, false));
                    }
                    if (derivatives[i] < 0.0 && derivatives[i - 1] >= 0.0)
                    {
                        inflectionIndex.Add((i, true));
                    }
                }

                for (int i = 0; i < inflectionIndex.Count - 1; ++i)
                {
                    if((inflectionIndex[i].BuySignal && 
                        stocks[inflectionIndex[i].Index].Close > stocks[inflectionIndex[i + 1].Index].Close) ||
                       (!inflectionIndex[i].BuySignal &&
                        stocks[inflectionIndex[i].Index].Close < stocks[inflectionIndex[i + 1].Index].Close))
                    {
                        inflectionIndex.RemoveAt(i);
                        inflectionIndex.RemoveAt(i);
                        --i;
                    }
                }

                for (int i = 0; i < inflectionIndex.Count; ++i)
                {
                    if(inflectionIndex[i].BuySignal)
                    {
                        //Find the low in the neighborhood
                        int updatedIndex = inflectionIndex[i].Index;
                        decimal minPrice = decimal.MaxValue;
                        for(int j = inflectionIndex[i].Index - _derivativeKernel / 2; j < inflectionIndex[i].Index + _derivativeKernel / 2; ++j)
                        {
                            if (j < 0 || j >= stocks.Count)
                                continue;

                            if(stocks[j].Low < minPrice)
                            {
                                updatedIndex = j;
                                minPrice = stocks[j].Low;
                            }
                        }

                        inflectionIndex[i] = (updatedIndex, true);
                    }
                    else
                    {
                        //Find the low in the neighborhood
                        int updatedIndex = inflectionIndex[i].Index;
                        decimal maxPrice = decimal.MinValue;
                        for (int j = inflectionIndex[i].Index - _derivativeKernel / 2; j < inflectionIndex[i].Index + _derivativeKernel / 2; ++j)
                        {
                            if (j < 0 || j >= stocks.Count)
                                continue;

                            if (stocks[j].High > maxPrice)
                            {
                                updatedIndex = j;
                                maxPrice = stocks[j].High;
                            }
                        }

                        inflectionIndex[i] = (updatedIndex, false);
                    }
                }

                for (int i = 0; i < inflectionIndex.Count - 3; ++i)
                {
                    //If buy and sell signal are too close to eachother, we'll need to filter some out
                    if(inflectionIndex[i + 1].Index - inflectionIndex[i].Index < _derivativeKernel / 2)
                    {
                        //Figure out which is the better signal
                        if(inflectionIndex[i].BuySignal)
                        {
                            if(stocks[inflectionIndex[i].Index].Low < stocks[inflectionIndex[i + 2].Index].Low)
                            {
                                //remove the lower buy signal
                                inflectionIndex.RemoveAt(i + 2);
                                if(stocks[inflectionIndex[i + 2].Index].Low > stocks[inflectionIndex[i + 1].Index].Low)
                                {
                                    inflectionIndex.RemoveAt(i + 1);
                                }
                                else
                                {
                                    inflectionIndex.RemoveAt(i + 2);
                                    i++;
                                }
                                
                            }
                            else
                            {
                                inflectionIndex.RemoveAt(i);
                                inflectionIndex.RemoveAt(i);
                            }
                        }
                        else
                        {
                            if (stocks[inflectionIndex[i].Index].High > stocks[inflectionIndex[i + 2].Index].High)
                            {
                                inflectionIndex.RemoveAt(i + 2);
                                if (stocks[inflectionIndex[i + 1].Index].High < stocks[inflectionIndex[i + 2].Index].High)
                                {
                                    inflectionIndex.RemoveAt(i + 2);
                                    i++;
                                }
                                else
                                {
                                    inflectionIndex.RemoveAt(i + 1);
                                }
                            }
                            else
                            {
                                inflectionIndex.RemoveAt(i);
                                inflectionIndex.RemoveAt(i);
                            }
                        }

                        --i;
                    }
                }

                //Add an offset
                for (int i = 0; i < inflectionIndex.Count; ++i)
                {
                    int offsettedIndex = inflectionIndex[i].Index + _signalOffset;
                    if (offsettedIndex < stocks.Count && offsettedIndex >= 0)
                        inflectionIndex[i] = (offsettedIndex, inflectionIndex[i].BuySignal);
                }

                var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
                var signalCheck = new List<double>();
                var priceCheck = new List<double>();
                double averageDistance = 0.0;
                for (int i = 1; i < inflectionIndex.Count; ++i)
                {
                    int j = inflectionIndex[i - 1].Index;
                    averageDistance += inflectionIndex[i].Index - j;

                    //decimal openPrice = inflectionIndex[i - 1].BuySignal ? stocks[j].Low : stocks[j].High;
                    //decimal closePrice = inflectionIndex[i - 1].BuySignal ? stocks[inflectionIndex[i].Index].High : stocks[inflectionIndex[i].Index].Low;

                    decimal openPrice = stocks[j].Open;
                    decimal closePrice = stocks[inflectionIndex[i].Index].Close;

                    while (j < inflectionIndex[i].Index)
                    {
                        var sample = stocks[j];

                        decimal signal = 0.0m;
                        if(openPrice == closePrice)
                        {
                            signal = inflectionIndex[i - 1].BuySignal ? 1.0m : 0.0m;
                        }
                        else if(inflectionIndex[i - 1].BuySignal)
                        {
                            signal = 1.0m - (sample.Average - openPrice) / (closePrice - openPrice);
                        }
                        else
                        {
                            signal = (sample.Average - openPrice) / (closePrice - openPrice);
                        }
                        if (signal > 1.0m)
                            signal = 1.0m;
                        if (signal < 0.0m)
                            signal = 0.0m;
                        if (filter.Pass(sample))
                        {
                            var data = GetData(symbol, sample.Date, stocks);
                            if (data.IsSuccess)
                            {
                                decimal high = 0.0m;
                                decimal low = decimal.MaxValue;
                                for (int l = j; l < inflectionIndex[i].Index; ++l)
                                {
                                    if (stocks[l].High > high)
                                    {
                                        high = stocks[l].High;
                                    }
                                    if (stocks[l].Low < low)
                                    {
                                        low = stocks[l].Low;
                                    }
                                }

                                int outputIndex = j + _predictionLength;
                                if(outputIndex >= stocks.Count)
                                {
                                    outputIndex = stocks.Count - 1;
                                }

                                var output = new StockData(symbol, sample.Date, sample.Open, high, low,
                                    stocks[outputIndex].Close,
                                    stocks[outputIndex].AdjustedClose,
                                    sample.Volume, sample.PreviousClose);

                                output.Signal = signal;
                                trainingData.Add((data.Value, output));
                                signalCheck.Add((double)signal);
                                priceCheck.Add((double)sample.Close);
                            }
                        }
                        ++j;
                    }
                }

                averageDistance /= inflectionIndex.Count - 1;
                if (!trainingData.Any())
                {
                    return new List<(FeatureVector Input, StockData Output)>();
                }

                return trainingData.OrderBy(x => x.Output.Date);
            }
            catch (Exception)
            {
                return new List<(FeatureVector Input, StockData Output)>();
            }
        }

        private double[] GetGaussianDerivativeKernel(int length)
        {
            double sigma = (double)length / 6.0;
            double e = 2.71828;
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
            var stocks = _stockRepository.GetStocks(symbol, Period).ToList();
            var features = new List<FeatureVector>();
            if (!stocks.Any())
            {
                return features;
            }

            foreach (var date in stocks)
            {
                var f = GetStockFeatureVector(date.Date, stocks, _numStockDailySamples);
                if (f.IsFailure)
                    continue;

                features.Add(new FeatureVector(f.Value, date.Date, _encodingKey));
            }

            return features;
        }

        public Result<FeatureVector> GetFeatureVector(string symbol, out StockData last, int historyLimit = 0)
        {
            var stocks = _stockRepository.GetStocks(symbol, Period, historyLimit).ToList();
            //remove the last candle because it hasn't been completely formed yet.
            var date = DateTime.UtcNow;
            if (!stocks.Any())
            {
                last = null;
                return Result.Failure<FeatureVector>(
                    $"No stock found for symbol '{symbol}' on {date.ToString("yyyy/MM/dd")}");
            }

            //stocks.RemoveAt(stocks.Count - 1);
            last = stocks.Last();
            return GetData(symbol, date, stocks);
        }

        public Result<FeatureVector> GetFeatureVector(IEnumerable<StockData> data, DateTime date)
        {
            if(!data.Any())
            {
                return Result.Failure<FeatureVector>("No data provided to compute feature vector");
            }
            return GetData(data.First().Symbol, date, data.ToList());
        }
    }
}
