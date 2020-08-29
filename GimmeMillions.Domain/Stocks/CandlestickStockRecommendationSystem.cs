using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.ML.Candlestick;
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
    public class CandlestickStockRecommendationSystem : IStockRecommendationSystem<FeatureVector>
    {
        private MLStockFastForestCandlestickModel model;
        private IFeatureDatasetService<FeatureVector> _featureDatasetService;
        private StockRecommendationSystemConfiguration _systemConfiguration;
        private string _pathToModels;

        public CandlestickStockRecommendationSystem(IFeatureDatasetService<FeatureVector> featureDatasetService,
            string pathToModels)
        {
            _featureDatasetService = featureDatasetService;
            _systemConfiguration = new StockRecommendationSystemConfiguration();
            _pathToModels = pathToModels;
        }

        public void AddModel(IStockPredictionModel<FeatureVector> stockPredictionModel)
        {
            _systemConfiguration.Models.Add(("ANY_STOCK",
                    _pathToModels,
                    stockPredictionModel.Encoding,
                    stockPredictionModel.GetType()));
        }

        public IEnumerable<StockRecommendation> GetAllRecommendations(DateTime date, bool updateStockHistory = false)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            if(updateStockHistory)
            {
                _featureDatasetService.StockAccess.UpdateFutures();
            }

            var stockSymbols = _featureDatasetService.StockAccess.GetSymbols();

            Parallel.ForEach(stockSymbols, symbol =>
            //foreach(var symbol in stockSymbols)
            {
                if (updateStockHistory)
                    _featureDatasetService.StockAccess.UpdateStocks(symbol);

                var feature = _featureDatasetService.GetFeatureVector(symbol, date);
                if (feature.IsFailure)
                {
                    return;
                }
                var result = model.Predict(feature.Value); 
                recommendations.Add(new StockRecommendation(symbol, result));
            //}
            });

            return recommendations.ToList().OrderByDescending(x => x.Prediction.Probability);
        }

        public IEnumerable<StockRecommendation> GetAllRecommendationsForToday(bool updateStockHistory = false)
        {
            return GetAllRecommendations(DateTime.Today);
        }

        public IEnumerable<StockRecommendation> GetRecommendations(DateTime date, int keepTop = 10, bool updateStockHistory = false)
        {
            var recommendations = GetAllRecommendations(date).Take(keepTop);
            var scoreSum = recommendations.Sum(x => x.Prediction.Score);
            foreach (var r in recommendations)
            {
                r.RecommendedInvestmentPercentage = r.Prediction.Score / scoreSum;
            }

            return recommendations;
        }

        public IEnumerable<StockRecommendation> GetRecommendationsFor(IEnumerable<string> symbols, DateTime date, bool updateStockHistory = false)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            Parallel.ForEach(symbols, symbol =>
            //foreach(var symbol in stockSymbols)
            {
                _featureDatasetService.StockAccess.UpdateStocks(symbol);

                var feature = _featureDatasetService.GetFeatureVector(symbol, date);
                if (feature.IsFailure)
                {
                    return;
                }
                var result = model.Predict(feature.Value);
                recommendations.Add(new StockRecommendation(symbol, result));
                //}
            });

            return recommendations.ToList().OrderByDescending(x => x.Prediction.Probability);
        }

        public IEnumerable<StockRecommendation> GetRecommendationsForToday(int keepTop = 10, bool updateStockHistory = false)
        {
            return GetRecommendations(DateTime.Today, keepTop);
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
            model = (MLStockFastForestCandlestickModel)Activator.CreateInstance(modelInfo.ModelType);
            var loadResult = model.Load(modelInfo.PathToModel, modelInfo.Symbol, modelInfo.Encoding);
            if (loadResult.IsSuccess)
            {
                return Result.Failure($"Model could not be loaded");
            }
        
            return Result.Ok();
        }

        public Result RetrainModels(DateTime startTime, DateTime endTime)
        {
            var trainingData = _featureDatasetService.GetAllTrainingData(startTime, endTime);
            var trainingResult = model.Train(trainingData, 0.0);
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
