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
    public class LambkinModelTrainer
    {
        private IFeatureDatasetService<FeatureVector> _datasetService;
        private IStockSymbolsRepository _stockSymbolsRepository;
        private IStockRangePredictor _model;
        private StockDataPeriod _period;
        private int _numStockSamples = 200;
        private int _predictionLength = 5;
        private string _apiKey;
        public LambkinModelTrainer(
            string apiKey,
            IStockSymbolsRepository stockSymbolsRepository,
            StockDataPeriod period,
            int kSize = 15,
            int numStockSamples = 200,
            int samplingPeriod = 12,
            int offset = 0,
            int predictionLength = 5)
        {
            _apiKey = apiKey;
            _stockSymbolsRepository = stockSymbolsRepository;
            _period = period;
            _numStockSamples = numStockSamples;
            _predictionLength = predictionLength;
            _datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(period, samplingPeriod, _numStockSamples, kSize, offset, _predictionLength);
        }

        public void LoadModel(string modelName)
        {
            _model = new DeepLearningStockRangePredictorModel(200, 1000, 1.0);
            _model.Load(modelName);
        }

        public IStockRangePredictor TrainFutures(string modelName, int numSamples)
        {
            _model = new MultiFeatureClassificationPredictorModel();

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(_datasetService.GetTrainingData("DIA", null, true, numSamples));
            trainingData.AddRange(_datasetService.GetTrainingData("SPY", null, true, numSamples));
            trainingData.AddRange(_datasetService.GetTrainingData("QQQ", null, true, numSamples));

            var medianGain = (double)trainingData.OrderBy(x => x.Output.PercentChangeFromPreviousClose)
               .ToList()[trainingData.Count / 2].Output.PercentChangeFromPreviousClose;
            var averageGain = (double)trainingData.Average(x => Math.Abs((double)x.Output.PercentChangeFromPreviousClose - medianGain));

            _model.Train(trainingData, 0.1, new SignalOutputMapper());
            _model.Save(modelName);

            return _model;
        }

        public IStockRangePredictor TrainStocks(string modelName, int numSamples)
        {
            //_model = new DeepLearningStockRangePredictorModel(200, 10000, 1.0);
            _model = new MultiFeatureClassificationPredictorModel();
            var stockFilter = new DefaultStockFilter(
                    maxPercentHigh: 50.0m,
                maxPercentLow: 50.0m,
                minPrice: 10.0m,
                maxPrice: decimal.MaxValue,
                minVolume: 100000.0m);

            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(_datasetService.GetAllTrainingData(stockFilter, true, numSamples));

            var medianGain = (double)trainingData.OrderBy(x => x.Output.PercentChangeFromPreviousClose)
                .ToList()[trainingData.Count / 2].Output.PercentChangeFromPreviousClose;
            var averageGain = (double)trainingData.Average(x => Math.Abs((double)x.Output.PercentChangeFromPreviousClose - medianGain));

            _model.Train(trainingData, 0.0, new SignalOutputMapper());
            _model.Save(modelName);

            return _model;
        }

        public double Evaluate(string outputDataFile, int numSamples, string symbol)
        {
            double accuracy = 0.0;
            using (var w = new StreamWriter(outputDataFile))
            {
                var testData = _datasetService.GetTrainingData(symbol, null, true, numSamples);
                var stocks = _datasetService.StockAccess.GetStocks(symbol, _period);
                foreach (var sample in testData)
                {
                    var prediction = _model.Predict(sample.Input);

                    if ((prediction.Sentiment > 50.0 && sample.Output.PercentChangeFromPreviousClose >= 0.0m) ||
                        (prediction.Sentiment < 50.0 && sample.Output.PercentChangeFromPreviousClose < 0.0m))
                        accuracy++;
                    var stockData = stocks.FirstOrDefault(x => x.Date == sample.Output.Date);
                    var line = string.Format($"{stockData.Close}\t{sample.Output.PercentChangeFromPreviousClose}\t{prediction.PredictedHigh}\t{prediction.PredictedLow}\t{prediction.Sentiment}");
                    w.WriteLine(line);
                    w.Flush();
                }

                accuracy /= (double)testData.Count();
            }

            return accuracy;
        }

        private IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
            StockDataPeriod period,
            int timeSampling = 10,
            int numStockSamples = 40,
            int kernelSize = 9,
            int signalOffset = 0,
            int predictionLength = 5)
        {
            var ameritradeClient = new TDAmeritradeApiClient(_apiKey);
            var stocksRepo = new TDAmeritradeStockAccessService(ameritradeClient, _stockSymbolsRepository);
            var extractor = new MultiStockFeatureExtractor(new List<IFeatureExtractor<StockData>>
            {
                new SupportResistanceStockFeatureExtractor(),
                new FibonacciStockFeatureExtractor(),
                new MACDHistogramFeatureExtraction(20),
                new TTMSqueezeFeatureExtraction(20),
                new RSIFeatureExtractor(20),
                new VWAPFeatureExtraction(20),
                new CMFFeatureExtraction(20),
                new BollingerBandFeatureExtraction(20),
                new KeltnerChannelFeatureExtraction(20),
                new TrendStockFeatureExtractor(20),
                new SimpleMovingAverageFeatureExtractor(20)
            });

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize, signalOffset, predictionLength);
        }
    }
}
