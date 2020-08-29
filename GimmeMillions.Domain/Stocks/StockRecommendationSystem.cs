using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRecommendationSystem<TFeature> : IStockRecommendationSystem<TFeature>
        where TFeature : FeatureVector
    {
        private List<IStockPredictionModel<TFeature>> _models;
        private IFeatureDatasetService<TFeature> _featureDatasetService;
        private StockRecommendationSystemConfiguration _systemConfiguration;
        private string _pathToModels;

        public StockRecommendationSystem(IFeatureDatasetService<TFeature> featureDatasetService,
            string pathToModels)
        {
            _models = new List<IStockPredictionModel<TFeature>>();
            _featureDatasetService = featureDatasetService;
            _systemConfiguration = new StockRecommendationSystemConfiguration();
            _pathToModels = pathToModels;
        }

        public void AddModel(IStockPredictionModel<TFeature> stockPredictionModel)
        {
            _models.Add(stockPredictionModel);
            _systemConfiguration.Models.Add((stockPredictionModel.StockSymbol, 
                _pathToModels,
                stockPredictionModel.Encoding, 
                stockPredictionModel.GetType()));
            //stockPredictionModel.Save(_pathToModels);
        }

        public IEnumerable<StockRecommendation> GetAllRecommendations(DateTime date, bool updateStockHistory = false)
        {
            var recommendations = new List<StockRecommendation>();
            var feature = _featureDatasetService.GetFeatureVector("", date);
            if (feature.IsFailure)
            {
                return recommendations;
            }

            foreach (var model in _models)
            {
                var result = model.Predict(feature.Value);
                recommendations.Add(new StockRecommendation(model.StockSymbol, result));
            }

            return recommendations.OrderByDescending(x => x.Prediction.Probability);
        }

        public IEnumerable<StockRecommendation> GetAllRecommendationsForToday(bool updateStockHistory = false)
        {
            return GetAllRecommendations(DateTime.Today, updateStockHistory);
        }

        public IEnumerable<StockRecommendation> GetRecommendations(DateTime date, int keepTop = 10, bool updateStockHistory = false)
        {
            var recommendations = GetAllRecommendations(date).Take(keepTop);
            var scoreSum = recommendations.Sum(x => x.Prediction.Score);
            foreach(var r in recommendations)
            {
                r.RecommendedInvestmentPercentage = r.Prediction.Score / scoreSum;
            }

            return recommendations;
        }

        public IEnumerable<StockRecommendation> GetRecommendationsForToday(int keepTop = 10, bool updateStockHistory = false)
        {
            return GetRecommendations(DateTime.Today, keepTop, updateStockHistory);
        }

        public Result LoadConfiguration(string configurationFile)
        {
            if (!File.Exists(configurationFile))
            {
                return Result.Failure($"Model configuration named {configurationFile} could not be found");
            }
            var json = File.ReadAllText(configurationFile);
            _systemConfiguration = JsonConvert.DeserializeObject<StockRecommendationSystemConfiguration>(json);

            _models = new List<IStockPredictionModel<TFeature>>();
            foreach (var models in _systemConfiguration.Models)
            {
                var model = (IStockPredictionModel<TFeature>)Activator.CreateInstance(models.ModelType);
                var loadResult = model.Load(models.PathToModel, models.Symbol, models.Encoding);
                if (loadResult.IsSuccess)
                {
                    _models.Add(model);
                }
                else
                {
                    return loadResult;
                }
            }

            return Result.Ok();
        }

        public Result RetrainModels(DateTime startTime, DateTime endTime)
        {
            foreach(var model in _models)
            {
                var trainingData = _featureDatasetService.GetTrainingData(model.StockSymbol, startTime, endTime);
                if(trainingData.IsFailure)
                {
                    return Result.Failure(trainingData.Error);
                }
                var trainingResult = model.Train(trainingData.Value, 0.0);
                if(trainingResult.IsFailure)
                {
                    return Result.Failure(trainingResult.Error);
                }
                model.Save(_pathToModels);
            }

            return Result.Ok();
        }

        public Result SaveConfiguration(string configurationFile)
        {
            File.WriteAllText(configurationFile, JsonConvert.SerializeObject(_systemConfiguration, Formatting.Indented));

            return Result.Ok();
        }
    }
}
