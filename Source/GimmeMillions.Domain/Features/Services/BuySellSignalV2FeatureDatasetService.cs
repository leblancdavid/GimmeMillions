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
    public class BuySellSignalV2FeatureDatasetService : IFeatureDatasetService<FeatureVector>
    {
        private IFeatureExtractor<StockData> _stockFeatureExtractor;
        private IStockAccessService _stockRepository;
        private int _numStockDailySamples = 20;
        private int _signalSampleLength = 15;
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


        public BuySellSignalV2FeatureDatasetService(IFeatureExtractor<StockData> stockFeatureExtractor,
            IStockAccessService stockRepository,
            StockDataPeriod period,
            int numStockDailySamples = 20,
            int signalSampleLength = 15)
        {
            _stockFeatureExtractor = stockFeatureExtractor;
            _stockRepository = stockRepository;

            _numStockDailySamples = numStockDailySamples;
            _signalSampleLength = signalSampleLength;
            
            string timeIndicator = $"{_numStockDailySamples}d-{_signalSampleLength}k";
            _stocksEncodingKey = $"{_stockFeatureExtractor.Encoding}_{timeIndicator}";
            _encodingKey = _stocksEncodingKey;
            Period = period;

        }

        public IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(
            IStockFilter filter = null,
            bool updateStocks = false, int historyLimit = 0)
        {
            var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
            var stockSymbols = _stockRepository.GetSymbols();

            var updateLock = new object();
            Parallel.ForEach(stockSymbols, symbol =>
            //foreach (var stock in stocks)
            {
                List<StockData> stocks = null;
                stocks = updateStocks ?
                      _stockRepository.UpdateStocks(symbol, Period).ToList() :
                      _stockRepository.GetStocks(symbol, Period).ToList();

                if (!stocks.Any())
                {
                    return;
                }

                var td = GetTrainingData(symbol, stocks, filter);
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
                var outputStock = stocks.FirstOrDefault(x => x.Date == date);
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
            bool updateStocks = false, int historyLimit = 0)
        {
            var stocks = updateStocks ?
                   _stockRepository.UpdateStocks(symbol, Period, historyLimit).ToList() :
                   _stockRepository.GetStocks(symbol, Period, historyLimit).ToList();
            if (!stocks.Any())
            {
                return new List<(FeatureVector Input, StockData Output)>();
            }


            return GetTrainingData(symbol, filter == null ? stocks : stocks.Where(x => filter.Pass(x)).ToList(), filter);

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

                var inflectionIndex = new List<(int Index, bool BuySignal)>();
                decimal averagePeriodChange = 0.0m;
                for(int i = 0; i < stocks.Count - _signalSampleLength; ++i)
                {
                    averagePeriodChange += stocks[i + _signalSampleLength].Close - stocks[i].Close;
                }
                averagePeriodChange /= stocks.Count - _signalSampleLength;

                var trend = new List<bool>();
                for (int i = 0; i < stocks.Count - _signalSampleLength; ++i)
                {
                    if(stocks[i + _signalSampleLength].Close - stocks[i].Close > averagePeriodChange)
                    {
                        trend.Add(true);
                    }
                    else
                    {
                        trend.Add(false);
                    }
                }
                
                for(int i = 0; i < trend.Count; ++i)
                {
                    if (trend[i])
                    {
                        decimal min = decimal.MaxValue;
                        int minIndex = i;
                        while(i < trend.Count && trend[i])
                        {
                            if(stocks[i].Close < min)
                            {
                                min = stocks[i].Close;
                                minIndex = i;
                            }
                            ++i;
                        }

                        inflectionIndex.Add((minIndex, true));
                    }
                    else
                    {
                        decimal max = decimal.MinValue;
                        int maxIndex = i;
                        while (i < trend.Count && !trend[i])
                        {
                            if (stocks[i].Close > max)
                            {
                                max = stocks[i].Close;
                                maxIndex = i;
                            }
                            ++i;
                        }

                        inflectionIndex.Add((maxIndex, false));
                    }
                }


                var trainingData = new ConcurrentBag<(FeatureVector Input, StockData Output)>();
                var signalCheck = new List<double>();
                var priceCheck = new List<double>();
                double averageDistance = 0.0;
                for (int i = 1; i < inflectionIndex.Count; ++i)
                {
                    int j = inflectionIndex[i - 1].Index;
                    averageDistance += inflectionIndex[i].Index - j;

                    decimal openPrice = stocks[j].Close;
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
                            signal = 1.0m - (sample.Close - openPrice) / (closePrice - openPrice);
                        }
                        else
                        {
                            signal = (sample.Close - openPrice) / (closePrice - openPrice);
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

                                var output = new StockData(symbol, sample.Date, sample.Open, high, low,
                                    stocks[inflectionIndex[i].Index - 1].Close,
                                    //sample.Close,
                                    stocks[inflectionIndex[i].Index - 1].AdjustedClose,
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
    }
}
