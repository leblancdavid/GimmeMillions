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

        public void AddModel(IStockPredictionModel<FeatureVector, StockRangePrediction> stockPredictionModel)
        {
            model = stockPredictionModel;
        }

        public IEnumerable<StockRecommendation> GetAllRecommendations(DateTime date, IStockFilter filter = null, bool updateStockHistory = false)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            if (updateStockHistory)
            {
                _featureDatasetService.StockAccess.UpdateFutures();
            }

            var stockSymbols = _featureDatasetService.StockAccess.GetSymbols();

            var saveLock = new object();
            Parallel.ForEach(stockSymbols, symbol =>
            //foreach(var symbol in stockSymbols)
            {
                List<StockData> stockData;
                if (updateStockHistory)
                    stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol).ToList();
                else
                    stockData = _featureDatasetService.StockAccess.GetStocks(symbol).ToList();

                if(!stockData.Any())
                {
                    //continue;
                    return;
                }

                stockData.Reverse();
                if(filter != null && !filter.Pass(StockData.Combine(stockData.Take(_filterLength))))
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
                lock(saveLock)
                {
                    _stockRecommendationRepository.AddRecommendation(rec);
                }
                //}
            });

            return recommendations.ToList().OrderByDescending(x => x.Sentiment);
        }

        public IEnumerable<StockRecommendation> GetAllRecommendationsForToday(IStockFilter filter = null, bool updateStockHistory = false)
        {
            return GetAllRecommendations(DateTime.Today, filter);
        }

        public IEnumerable<StockRecommendation> GetRecommendations(DateTime date, IStockFilter filter = null, int keepTop = 10, bool updateStockHistory = false)
        {
            var recommendations = GetAllRecommendations(date, filter).Take(keepTop);
            return recommendations;
        }

        public IEnumerable<StockRecommendation> GetRecommendationsFor(IEnumerable<string> symbols, DateTime date, IStockFilter filter = null, bool updateStockHistory = false)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();
            if (updateStockHistory)
                _featureDatasetService.StockAccess.UpdateFutures();

            var saveLock = new object();
            Parallel.ForEach(symbols, symbol =>
            //foreach(var symbol in symbols)
            {
                List<StockData> stockData;
                if (updateStockHistory)
                    stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol).ToList();
                else
                    stockData = _featureDatasetService.StockAccess.GetStocks(symbol).ToList();

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

            return recommendations.ToList().OrderByDescending(x => x.Sentiment);
        }

        public IEnumerable<StockRecommendation> GetRecommendationsForToday(IStockFilter filter = null, int keepTop = 10, bool updateStockHistory = false)
        {
            return GetRecommendations(DateTime.Today, filter, keepTop);
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

            return Result.Ok();
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

            return Result.Ok();
        }

        public Result SaveConfiguration(string configurationFile)
        {
            File.WriteAllText(configurationFile, JsonConvert.SerializeObject(_systemConfiguration, Formatting.Indented));

            return Result.Ok();
        }
    }
}
