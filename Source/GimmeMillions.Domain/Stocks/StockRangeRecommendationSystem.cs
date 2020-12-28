﻿using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks.Filters;
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
        private IStockRecommendationRepository _stockRecommendationRepository;
        private StockRecommendationSystemConfiguration _systemConfiguration;
        private string _pathToModels;
        private string _systemId;
        private int _filterLength = 3;

        public StockRangeRecommendationSystem(IFeatureDatasetService<FeatureVector> featureDatasetService,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModels,
            string systemId,
            int filterLength)
        {
            _featureDatasetService = featureDatasetService;
            _stockRecommendationRepository = stockRecommendationRepository;
            _systemConfiguration = new StockRecommendationSystemConfiguration();
            _pathToModels = pathToModels;
            _systemId = systemId;
            _filterLength = filterLength;
        }

        public IStockRecommendationRepository RecommendationRepository => _stockRecommendationRepository;
        public string SystemId => _systemId;

        public void AddModel(IStockPredictionModel<FeatureVector, StockRangePrediction> stockPredictionModel)
        {
            model = stockPredictionModel;
        }

        public IEnumerable<StockRecommendation> RunAllRecommendations(DateTime date, IStockFilter filter = null)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            var stockSymbols = _featureDatasetService.StockAccess.GetSymbols();

            var saveLock = new object();
            Parallel.ForEach(stockSymbols, symbol =>
            //foreach(var symbol in stockSymbols)
            {
                List<StockData> stockData;
                stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

                if (!stockData.Any())
                {
                    //continue;
                    return;
                }

                stockData.Reverse();
                if (filter != null && !filter.Pass(StockData.Combine(stockData.Take(_filterLength))))
                {
                    //continue;
                    return;
                }
                var lastStock = stockData.First();

                var feature = _featureDatasetService.GetFeatureVector(symbol, date);
                if (feature.IsFailure)
                {
                    //continue;
                    return;
                }
                var result = model.Predict(feature.Value);
                var rec = new StockRecommendation(_systemId, date, symbol,
                    (decimal)result.PredictedHigh, (decimal)result.PredictedLow, (decimal)result.Sentiment, lastStock.Close);
                recommendations.Add(rec);
                lock (saveLock)
                {
                    _stockRecommendationRepository.AddRecommendation(rec);
                }
            //}
            });

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

            var saveLock = new object();
            try
            {
                Parallel.ForEach(symbols, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, symbol =>
                //foreach(var symbol in symbols)
                {
                    List<StockData> stockData;
                    stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

                    if (!stockData.Any())
                    {
                        //continue;
                        return;
                    }

                    stockData.Reverse();
                    if (filter != null && !filter.Pass(StockData.Combine(stockData.Take(_filterLength))))
                    {
                        //continue;
                        return;
                    }
                    var lastStock = stockData.First();

                    var feature = _featureDatasetService.GetFeatureVector(symbol, date);
                    if (feature.IsFailure)
                    {
                        //continue;
                        return;
                    }
                    var result = model.Predict(feature.Value);
                    var rec = new StockRecommendation(_systemId, date, symbol,
                        (decimal)result.PredictedHigh, (decimal)result.PredictedLow,
                        (decimal)result.Sentiment, lastStock.Close);
                    recommendations.Add(rec);

                    lock (saveLock)
                    {
                        _stockRecommendationRepository.AddRecommendation(rec);
                    }
                //}
                });
            }
            catch(Exception ex)
            {
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
            List<StockData> stockData;
            stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

            if (!stockData.Any())
            {
                return Result.Failure<StockRecommendation>($"No stock data found for {symbol}");
            }

            stockData.Reverse();
            var lastStock = stockData.First();

            var feature = _featureDatasetService.GetFeatureVector(symbol, date);
            if (feature.IsFailure)
            {
                return Result.Failure<StockRecommendation>($"Unable to compute feature vector for {symbol}");
            }
            var result = model.Predict(feature.Value);
            var rec = new StockRecommendation(_systemId, date, symbol,
                (decimal)result.PredictedHigh, (decimal)result.PredictedLow,
                (decimal)result.Sentiment, lastStock.Close);

            var saveLock = new object();
            lock (saveLock)
            {
                _stockRecommendationRepository.AddRecommendation(rec);
            }
            return Result.Success<StockRecommendation>(rec);
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