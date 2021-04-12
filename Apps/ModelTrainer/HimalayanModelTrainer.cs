using GimmeMillions.DataAccess.Clients.TDAmeritrade;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Features.Extractors;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.ML.Candlestick;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModelTrainer
{
    public class HimalayanModelTrainer
    {
        private IFeatureDatasetService<FeatureVector> _datasetService;
        private IStockSymbolsRepository _stockSymbolsRepository;
        private IStockRangePredictor _model;
        private StockDataPeriod _period;
        private int _numStockSamples = 100;
        public HimalayanModelTrainer(
            IStockSymbolsRepository stockSymbolsRepository,
            StockDataPeriod period,
            int kSize = 15,
            int numStockSamples = 100,
            int samplingPeriod = 12,
            int offset = 0)
        {
            _stockSymbolsRepository = stockSymbolsRepository;
            _period = period;
            _numStockSamples = numStockSamples;
            _datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(period, samplingPeriod, _numStockSamples, kSize, offset);
        }

        public void LoadModel(string modelName)
        {
            _model = new DeepLearningStockRangePredictorModel(200, 1000, 2.0);
            _model.Load(modelName);
        }

        public IStockRangePredictor TrainFutures(string modelName, int numSamples)
        {
            _model = new DeepLearningStockRangePredictorModel(200, 1000, 2.0);

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(_datasetService.GetTrainingData("DIA", null, true, numSamples));
            trainingData.AddRange(_datasetService.GetTrainingData("SPY", null, true, numSamples));
            trainingData.AddRange(_datasetService.GetTrainingData("QQQ", null, true, numSamples));
            //trainingData.AddRange(datasetService.GetTrainingData("RUT", null, true, numSamples));

            _model.Train(trainingData, 0.0, new SignalOutputMapper());
            _model.Save(modelName);

            return _model;
        }

        public IStockRangePredictor TrainStocks(string modelName, int numSamples)
        {
            _model = new DeepLearningStockRangePredictorModel(100, 5000, 2.0);

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(_datasetService.GetAllTrainingData(null, true, numSamples));
            _model.Train(trainingData, 0.0, new SignalOutputMapper());
            _model.Save(modelName);

            return _model;
        }

        public void Evaluate(string outputDataFile, int numSamples, string symbol)
        {
            using (var w = new StreamWriter(outputDataFile))
            {
                var testData = _datasetService.GetTrainingData(symbol, null, true, numSamples);
                var stocks = _datasetService.StockAccess.GetStocks(symbol, _period);
                foreach(var sample in testData)
                {
                    var prediction = _model.Predict(sample.Input);

                    var stockData = stocks.FirstOrDefault(x => x.Date == sample.Output.Date);
                    var line = string.Format($"{stockData.Close}\t{sample.Output.Signal}\t{sample.Output.PercentChangeFromPreviousClose}\t{prediction.PredictedHigh}\t{prediction.PredictedLow}\t{prediction.Sentiment}\t{prediction.Confidence}");
                    w.WriteLine(line);
                    w.Flush();
                }
            }
        }

        private IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
            StockDataPeriod period,
            int timeSampling = 10,
            int numStockSamples = 40,
            int kernelSize = 9,
            int signalOffset = 0)
        {
            var ameritradeClient = new TDAmeritradeApiClient("I12BJE0PV9ARIGTWWOPJGCGRWPBUJLRP");
            var stocksRepo = new TDAmeritradeStockAccessService(ameritradeClient, _stockSymbolsRepository);
            var extractor = new MultiStockFeatureExtractor(new List<IFeatureExtractor<StockData>>
            {
                //Remove the fibonacci because I believe they may not be reliable
                new FibonacciStockFeatureExtractor(),
                new TrendStockFeatureExtractor(numStockSamples / 2),
                new StockIndicatorsFeatureExtractionV3(timeSampling,
                numStockSamples,
                (int)(numStockSamples * 0.8), (int)(numStockSamples * 0.4), (int)(numStockSamples * 0.3), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                (int)(numStockSamples * 0.8), 5,
                false)
            });

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize, signalOffset);
        }

    }
}
