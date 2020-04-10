using FluentAssertions;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Candlestick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.Domain.Tests.ML
{
    public class MLStockFastForestCandlestickModelTests
    {
        private readonly string _pathToArticles = "../../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../../Repository/Languages";
        private readonly string _pathToStocks = "../../../../../Repository/Stocks";
        private readonly string _pathToCache = "../../../../../Repository/Cache";
        private readonly string _pathToModels = "../../../../../Repository/Models";
        private readonly string _pathToKeys = "../../../../Repository/Keys";

        [Fact]
        public void ShouldTrainUsingCandlestickFeatures()
        {
            var datasetService = GetCandlestickFeatureDatasetService();
            var model = new MLStockFastForestCandlestickModel();
            model.Parameters.NumCrossValidations = 10;
            model.Parameters.NumOfTrees = 1000;
            model.Parameters.NumOfLeaves = 20;
            model.Parameters.MinNumOfLeaves = 20;

            var dataset = datasetService.GetAllTrainingData(new DateTime(2000, 1, 1), new DateTime(2020, 4, 9));
            dataset.Any().Should().BeTrue();

            var trainingResults = model.Train(dataset, 0.1);
        }

 
        private IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetService()
        {
            var stocksRepo = new YahooFinanceStockAccessService(new StockDataRepository(_pathToStocks), _pathToStocks);

            var cache = new FeatureJsonCache<FeatureVector>(_pathToCache);
            var featureExtractor = new CandlestickStockFeatureExtractorV2();
            int numberSamples = 20;
            return new CandlestickStockFeatureDatasetService(featureExtractor, stocksRepo, cache, numberSamples);
        }
    }
}
