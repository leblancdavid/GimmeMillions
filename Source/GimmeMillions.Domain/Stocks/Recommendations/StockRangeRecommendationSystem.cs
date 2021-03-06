﻿using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks.Filters;
using GimmeMillions.Domain.Stocks.Recommendations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRangeRecommendationSystem : IStockRecommendationSystem<FeatureVector>
    {
        private IStockPredictionModel<FeatureVector, StockRangePrediction> model;
        private IFeatureDatasetService<FeatureVector> _featureDatasetService;
        private IStockRecommendationHistoryRepository _stockRecommendationRepository;
        private StockRecommendationSystemConfiguration _systemConfiguration;
        private string _pathToModels;
        private string _systemId;
        private int _filterLength = 3;
        private int _historyLength = 5;
        private ILogger _logger;

        public StockRangeRecommendationSystem(IFeatureDatasetService<FeatureVector> featureDatasetService,
            IStockRecommendationHistoryRepository stockRecommendationRepository,
            string pathToModels,
            string systemId,
            int filterLength,
            int historyLength,
            ILogger logger)
        {
            _featureDatasetService = featureDatasetService;
            _stockRecommendationRepository = stockRecommendationRepository;
            _systemConfiguration = new StockRecommendationSystemConfiguration();
            _pathToModels = pathToModels;
            _systemId = systemId;
            _filterLength = filterLength;
            _historyLength = historyLength;
            _logger = logger;
        }

        public IStockRecommendationHistoryRepository RecommendationRepository => _stockRecommendationRepository;
        public string SystemId => _systemId;

        public void AddModel(IStockPredictionModel<FeatureVector, StockRangePrediction> stockPredictionModel)
        {
            model = stockPredictionModel;
        }

        public IEnumerable<StockRecommendation> RunAllRecommendations(DateTime date, IStockFilter filter = null)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            var stockSymbols = _featureDatasetService.StockAccess.GetSymbols();

            _logger.LogInformation($"Running all recommendations for {date.ToString()}");
            var saveLock = new object();
            //Parallel.ForEach(stockSymbols, symbol =>
            foreach(var symbol in stockSymbols)
            {
                List<StockData> stockData;
                stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

                if (!stockData.Any())
                {
                    _logger?.LogInformation($"{symbol}: No historical data found");
                    continue;
                    //return;
                }

                if (filter != null && !filter.Pass(StockData.Combine(stockData.Skip(stockData.Count - _filterLength))))
                {
                    _logger?.LogInformation($"{symbol}: Does not pass filter requirements");
                    continue;
                    //return;
                }

                var lastStock = stockData.Last();
                if (lastStock.Date < date.AddDays(-5.0))
                {
                    _logger?.LogInformation($"{symbol}: Historical data is not up to date");
                    continue;
                }

                StockRecommendationHistory history = null;
                var existingHistory = _stockRecommendationRepository.GetStockRecommendationHistory(_systemId, symbol);
                if(existingHistory.IsSuccess)
                {
                    history = existingHistory.Value;
                }
                else
                {
                    history = new StockRecommendationHistory(_systemId, symbol, new List<StockRecommendation>());
                }


                int startingIndex = stockData.FindIndex(x => x.Date.Date == date.Date);
                if (startingIndex < 0)
                {
                    startingIndex = stockData.Count;
                }

                for (int i = 0; i < _historyLength; ++i)
                {
                    int si = startingIndex - i;
                    if (si < 1)
                        break;

                    DateTime recommendationDate = date;
                    if (si < stockData.Count)
                    {
                        recommendationDate = stockData[si].Date;
                        if(history.ContainsEntryFor(recommendationDate))
                        {
                            continue;
                        }
                    }

                    var feature = _featureDatasetService.GetData(symbol, recommendationDate, stockData);
                    if (feature.IsFailure)
                    {
                        _logger?.LogInformation($"{symbol}: Unable to compute the feature vector: {feature.Error}");
                        break;
                        //return;
                    }

                    var result = model.Predict(feature.Value);
                    var rec = new StockRecommendation(_systemId,
                        (decimal)result.PredictedHigh, (decimal)result.PredictedLow, (decimal)result.Sentiment, (decimal)result.Confidence,
                        recommendationDate, stockData[si - 1]);

                    history.AddOrUpdateRecommendation(rec);

                }

                if(history.LastRecommendation != null)
                {
                    var text = $"{history.LastRecommendation.Symbol}, " +
                        $"({Math.Round(history.LastRecommendation.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                       $"gain: {Math.Round(history.LastRecommendation.Prediction, 2, MidpointRounding.AwayFromZero)}%, " +
                       $"high: {Math.Round(history.LastRecommendation.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                       $"loss: {Math.Round(history.LastRecommendation.LowPrediction, 2, MidpointRounding.AwayFromZero)}%, " +
                       $"low: {Math.Round(history.LastRecommendation.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}, " +
                       $"conf: {Math.Round(history.LastRecommendation.Confidence, 2, MidpointRounding.AwayFromZero)}";
                    _logger.LogInformation($"Updating {symbol}: {text}");
                    recommendations.Add(history.LastRecommendation);
                    lock (saveLock)
                    {
                        var addResult = _stockRecommendationRepository.AddOrUpdateRecommendationHistory(history);
                        if (addResult.IsFailure)
                        {
                            _logger?.LogError($"Unable to add recommendation: '{addResult.Error}'");
                        }
                    }
                }
                
            }
            //});

            return recommendations.ToList().OrderByDescending(x => x.Sentiment);
        }

        public IEnumerable<StockRecommendation> RunAllRecommendationsForToday(IStockFilter filter = null)
        {
            return RunAllRecommendations(DateTime.Today, filter);
        }

        public IEnumerable<StockRecommendation> RunRecommendations(DateTime date, IStockFilter filter = null, int keepTop = 10)
        {
            var recommendations = RunAllRecommendations(date, filter).Take(keepTop);
            return recommendations;
        }

        public IEnumerable<StockRecommendation> RunRecommendationsFor(IEnumerable<string> symbols, DateTime date, IStockFilter filter = null)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            _logger?.LogInformation($"Running recommendations for {date.ToString()}");
            var saveLock = new object();
            try
            {
                //Parallel.ForEach(symbols, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, symbol =>
                foreach(var symbol in symbols)
                {
                    List<StockData> stockData;
                    stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

                    if (!stockData.Any())
                    {
                        _logger?.LogInformation($"{symbol}: No historical data found");
                        continue;
                        //return;
                    }

                    if (filter != null && !filter.Pass(StockData.Combine(stockData.Skip(stockData.Count - _filterLength))))
                    {
                        _logger?.LogInformation($"{symbol}: Does not pass filter requirements");
                        continue;
                        //return;
                    }
                    var lastStock = stockData.Last();
                    if (lastStock.Date < date.AddDays(-5.0))
                    {
                        _logger?.LogInformation($"{symbol}: Historical data is not up to date");
                        continue;
                    }

                    StockRecommendationHistory history = null;
                    var existingHistory = _stockRecommendationRepository.GetStockRecommendationHistory(_systemId, symbol);
                    if (existingHistory.IsSuccess)
                    {
                        history = existingHistory.Value;
                    }
                    else
                    {
                        history = new StockRecommendationHistory(_systemId, symbol, new List<StockRecommendation>());
                    }

                    int startingIndex = stockData.FindIndex(x => x.Date.Date == date.Date);
                    if(startingIndex < 0)
                    {
                        startingIndex = stockData.Count;
                    }

                    for (int i = 0; i < _historyLength; ++i)
                    {
                        int si = startingIndex - i;
                        if (si < 1)
                            break;

                        DateTime recommendationDate = date;
                        if (si < stockData.Count)
                        {
                            recommendationDate = stockData[si].Date;
                            if (history.ContainsEntryFor(recommendationDate))
                            {
                                continue;
                            }
                        }

                        var feature = _featureDatasetService.GetData(symbol, recommendationDate, stockData);
                        if (feature.IsFailure)
                        {
                            _logger?.LogInformation($"{symbol}: Unable to compute the feature vector: {feature.Error}");
                            break;
                            //return;
                        }

                        var result = model.Predict(feature.Value);
                        var rec = new StockRecommendation(_systemId,
                            (decimal)result.PredictedHigh, (decimal)result.PredictedLow, (decimal)result.Sentiment, (decimal)result.Confidence,
                            recommendationDate, stockData[si - 1]);

                        history.AddOrUpdateRecommendation(rec);

                    }

                    if (history.LastRecommendation != null)
                    {
                        var text = $"{history.LastRecommendation.Symbol}, " +
                            $"({Math.Round(history.LastRecommendation.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                           $"gain: {Math.Round(history.LastRecommendation.Prediction, 2, MidpointRounding.AwayFromZero)}%, " +
                           $"high: {Math.Round(history.LastRecommendation.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                           $"loss: {Math.Round(history.LastRecommendation.LowPrediction, 2, MidpointRounding.AwayFromZero)}%, " +
                           $"low: {Math.Round(history.LastRecommendation.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}, " +
                            $"conf: {Math.Round(history.LastRecommendation.Confidence, 2, MidpointRounding.AwayFromZero)}";
                        _logger.LogInformation($"Updating {symbol}: {text}");
                        recommendations.Add(history.LastRecommendation);
                        lock (saveLock)
                        {
                            var addResult = _stockRecommendationRepository.AddOrUpdateRecommendationHistory(history);
                            if (addResult.IsFailure)
                            {
                                _logger?.LogError($"Unable to add recommendation: '{addResult.Error}'");
                            }
                        }
                    }
                }
                //});
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex.Message);
                throw new Exception(ex.Message);
            }

            return recommendations.ToList().OrderByDescending(x => x.Sentiment);
        }

        public IEnumerable<StockRecommendation> RunRecommendationsForToday(IStockFilter filter = null, int keepTop = 10, bool updateStockHistory = false)
        {
            return RunRecommendations(DateTime.Today, filter, keepTop);
        }

        public Result LoadConfiguration(string configurationFile)
        {
            if (!File.Exists(configurationFile))
            {
                return Result.Failure($"Model configuration named {configurationFile} could not be found");
            }
            var json = File.ReadAllText(configurationFile);
            _systemConfiguration = JsonConvert.DeserializeObject<StockRecommendationSystemConfiguration>(json);

            var modelInfo = _systemConfiguration.Models.FirstOrDefault();
            model = (IStockPredictionModel<FeatureVector, StockRangePrediction>)Activator.CreateInstance(modelInfo.ModelType);
            var loadResult = model.Load(modelInfo.PathToModel);
            if (loadResult.IsSuccess)
            {
                return Result.Failure($"Model could not be loaded");
            }

            return Result.Success();
        }

        public Result RetrainModels(DateTime startTime, DateTime endTime)
        {
            var trainingData = _featureDatasetService.GetAllTrainingData();
            var trainingResult = model.Train(trainingData, 0.0, null);
            if (trainingResult.IsFailure)
            {
                return Result.Failure(trainingResult.Error);
            }
            model.Save(_pathToModels);

            return Result.Success();
        }

        public Result SaveConfiguration(string configurationFile)
        {
            File.WriteAllText(configurationFile, JsonConvert.SerializeObject(_systemConfiguration, Formatting.Indented));

            return Result.Success();
        }

        public IEnumerable<StockRecommendation> GetRecommendationsForToday(int keep)
        {
            return GetRecommendations(DateTime.Today, keep);
        }

        public IEnumerable<StockRecommendation> GetRecommendations(DateTime date, int keep)
        {
            if(keep <= 0)
            {
                return _stockRecommendationRepository.GetStockRecommendations(_systemId, date);
            }

            return _stockRecommendationRepository.GetStockRecommendations(_systemId, date).Take(keep);
        }

        public Result<StockRecommendation> GetRecommendation(DateTime date, string symbol)
        {
            var result = _stockRecommendationRepository.GetStockRecommendation(_systemId, symbol, date);
            if(result.IsSuccess)
            {
                return result;
            }

            return RunRecommendationsFor(symbol, date);
        }

        public Result<StockRecommendation> RunRecommendationsFor(string symbol, DateTime date)
        {
            _logger.LogInformation($"Running recommendation for {symbol} on {date.ToString()}");
            List<StockData> stockData;
            stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

            if (!stockData.Any())
            {
                return Result.Failure<StockRecommendation>($"No stock data found for {symbol}");
            }

            StockRecommendationHistory history = null;
            var existingHistory = _stockRecommendationRepository.GetStockRecommendationHistory(_systemId, symbol);
            if (existingHistory.IsSuccess)
            {
                history = existingHistory.Value;
            }
            else
            {
                history = new StockRecommendationHistory(_systemId, symbol, new List<StockRecommendation>());
            }

            int startingIndex = stockData.FindIndex(x => x.Date.Date == date.Date);
            if (startingIndex < 0)
            {
                startingIndex = stockData.Count;
            }

            for (int i = 0; i < _historyLength; ++i)
            {
                int si = startingIndex - i;
                if (si < 1)
                    break;

                DateTime recommendationDate = date;
                if (si < stockData.Count)
                {
                    recommendationDate = stockData[si].Date;
                    if (history.ContainsEntryFor(recommendationDate))
                    {
                        continue;
                    }
                }

                var feature = _featureDatasetService.GetData(symbol, recommendationDate, stockData);
                if (feature.IsFailure)
                {
                    _logger?.LogInformation($"{symbol}: Unable to compute the feature vector: {feature.Error}");
                    break;
                    //return;
                }

                var result = model.Predict(feature.Value);
                var rec = new StockRecommendation(_systemId,
                    (decimal)result.PredictedHigh, (decimal)result.PredictedLow, (decimal)result.Sentiment, (decimal)result.Confidence,
                    recommendationDate, stockData[si - 1]);

                history.AddOrUpdateRecommendation(rec);

            }

            if (history.LastRecommendation != null)
            {
                var text = $"{history.LastRecommendation.Symbol}, " +
                    $"({Math.Round(history.LastRecommendation.Sentiment, 2, MidpointRounding.AwayFromZero)}%) - " +
                   $"gain: {Math.Round(history.LastRecommendation.Prediction, 2, MidpointRounding.AwayFromZero)}%, " +
                   $"high: {Math.Round(history.LastRecommendation.PredictedPriceTarget, 2, MidpointRounding.AwayFromZero)}, " +
                   $"loss: {Math.Round(history.LastRecommendation.LowPrediction, 2, MidpointRounding.AwayFromZero)}%, " +
                   $"low: {Math.Round(history.LastRecommendation.PredictedLowTarget, 2, MidpointRounding.AwayFromZero)}, " +
                    $"conf: {Math.Round(history.LastRecommendation.Confidence, 2, MidpointRounding.AwayFromZero)}";
                _logger.LogInformation($"Updating {symbol}: {text}");
                var saveLock = new object();
                lock (saveLock)
                {
                    var addResult = _stockRecommendationRepository.AddOrUpdateRecommendationHistory(history);
                    if (addResult.IsFailure)
                    {
                        _logger?.LogError($"Unable to add recommendation: '{addResult.Error}'");
                    }
                }
            }

            return Result.Success<StockRecommendation>(history.LastRecommendation);
        }

        public IEnumerable<StockRecommendation> GetRecommendations(IEnumerable<string> symbols, DateTime date)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            var saveLock = new object();
            //Parallel.ForEach(symbols, symbol =>
            foreach (var symbol in symbols)
            {
                var r = GetRecommendation(date, symbol);
                if (r.IsSuccess)
                {
                    recommendations.Add(r.Value);
                }
            }
            //});

            return recommendations.ToList().OrderByDescending(x => x.Sentiment);
        }
    }
}