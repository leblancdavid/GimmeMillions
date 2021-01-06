using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.ML.Candlestick;
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
    public class CandlestickStockRecommendationSystem : IStockRecommendationSystem<FeatureVector>
    {
        private IStockPredictionModel<FeatureVector, StockPrediction> model;
        private IFeatureDatasetService<FeatureVector> _featureDatasetService;
        private IStockRecommendationRepository _stockRecommendationRepository;
        private StockRecommendationSystemConfiguration _systemConfiguration;
        private string _pathToModels;
        private string _systemId;

        public CandlestickStockRecommendationSystem(IFeatureDatasetService<FeatureVector> featureDatasetService,
            IStockRecommendationRepository stockRecommendationRepository,
            string pathToModels,
            string systemId)
        {
            _featureDatasetService = featureDatasetService;
            _stockRecommendationRepository = stockRecommendationRepository;
            _systemConfiguration = new StockRecommendationSystemConfiguration();
            _pathToModels = pathToModels;
            _systemId = systemId;
        }

        public IStockRecommendationRepository RecommendationRepository => _stockRecommendationRepository;

        public string SystemId => _systemId;

        public void AddModel(IStockPredictionModel<FeatureVector, StockPrediction> stockPredictionModel)
        {
            //_systemConfiguration.Models.Add(("ANY_STOCK",
            //        _pathToModels,
            //        stockPredictionModel.Encoding,
            //        stockPredictionModel.GetType()));
            model = stockPredictionModel;
        }

        public IEnumerable<StockRecommendation> RunAllRecommendations(DateTime date, IStockFilter filter = null)
        {
            var recommendations = new ConcurrentBag<StockRecommendation>();

            var stockSymbols = _featureDatasetService.StockAccess.GetSymbols();

            Parallel.ForEach(stockSymbols, symbol =>
            //foreach(var symbol in stockSymbols)
            {
                List<StockData> stockData;
                stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

                var lastStock = stockData.Where(x => x.Date < date).Last();

                var feature = _featureDatasetService.GetFeatureVector(symbol, date);
                if (feature.IsFailure)
                {
                    //continue;
                    return;
                }
                var result = model.Predict(feature.Value);
                var rec = new StockRecommendation(_systemId, date, symbol,
                    (decimal)result.Probability, lastStock.Close);
                recommendations.Add(rec);
                _stockRecommendationRepository.AddOrUpdateRecommendation(rec);
                //}
            });

            return recommendations.ToList().OrderByDescending(x => x.Prediction);
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

            Parallel.ForEach(symbols, symbol =>
            //foreach(var symbol in symbols)
            {
                List<StockData> stockData;
                stockData = _featureDatasetService.StockAccess.UpdateStocks(symbol, _featureDatasetService.Period).ToList();

                var lastStock = stockData.Where(x => x.Date < date).LastOrDefault();
                if (lastStock == null)
                {
                    //continue;
                    return;
                }

                var feature = _featureDatasetService.GetFeatureVector(symbol, date);
                if (feature.IsFailure)
                {
                    //continue;
                    return;
                }
                var result = model.Predict(feature.Value);
                var rec = new StockRecommendation(_systemId, date, symbol,
                    (decimal)result.Probability, lastStock.Close);
                recommendations.Add(rec);
                _stockRecommendationRepository.AddOrUpdateRecommendation(rec);
                //}
            });

            return recommendations.ToList().OrderByDescending(x => x.Prediction);
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
            model = (MLStockFastForestCandlestickModel)Activator.CreateInstance(modelInfo.ModelType);
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
            return _stockRecommendationRepository.GetStockRecommendations(_systemId, date).Take(keep);
        }

        public Result<StockRecommendation> GetRecommendation(DateTime date, string symbol)
        {
            return _stockRecommendationRepository.GetStockRecommendation(_systemId, symbol, date);
        }

        public Result<StockRecommendation> RunRecommendationsFor(string symbol, DateTime date)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockRecommendation> GetRecommendations(IEnumerable<string> symbols, DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}