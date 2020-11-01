using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNTrainer
{
    public class CatModelTrainer
    {
        private IStockRepository _stockRepository;
        private IDatasetFilter _filter;
        public CatModelTrainer(IStockRepository stockRepository, IDatasetFilter filter)
        {
            _stockRepository = stockRepository;
            _filter = filter;

        }
        public void Train(string modelFile)
        {
            var datasetService = GetCandlestickFeatureDatasetServiceV3(20, 5);

            var model = new MLStockRangePredictorModel();

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(datasetService.GetAllTrainingData(_filter, true));
            var averageGrowth = trainingData.Average(x => x.Output.PercentChangeFromPreviousClose);

            var trainingResults = model.Train(trainingData, 0.0, new PercentDayChangeOutputMapper(averageGrowth));
            model.Save(modelFile);
        }

        private IFeatureDatasetService<FeatureVector> GetCandlestickFeatureDatasetServiceV3(
           int numStockSamples = 40,
           int kernelSize = 9)
        {
            var stocksRepo = new YahooFinanceStockAccessService(_stockRepository);
            var extractor = new RawStockFeatureExtractor();
            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                numStockSamples, kernelSize);
        }
    }
}
