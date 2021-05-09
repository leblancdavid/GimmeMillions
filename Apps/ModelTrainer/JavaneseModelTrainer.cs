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
    public class JavaneseModelTrainer
    {
        private IFeatureDatasetService<FeatureVector> _datasetService;
        private IStockSymbolsRepository _stockSymbolsRepository;
        private IStockRangePredictor _model;
        private StockDataPeriod _period;
        private int _numStockSamples = 100;
        private int _predictionLength = 5;
        public JavaneseModelTrainer(
            IStockSymbolsRepository stockSymbolsRepository,
            StockDataPeriod period,
            int kSize = 15,
            int numStockSamples = 100,
            int samplingPeriod = 12,
            int offset = 0,
            int predictionLength = 5)
        {
            _stockSymbolsRepository = stockSymbolsRepository;
            _period = period;
            _numStockSamples = numStockSamples;
            _predictionLength = predictionLength;
            _datasetService = GetIndicatorFeaturesBuySellSignalDatasetService(period, samplingPeriod, _numStockSamples, kSize, offset, _predictionLength);
        }

        public void LoadModel(string modelName)
        {
            _model = new DeepLearningStockRangePredictorModel(200, 1000, 2.0);
            _model.Load(modelName);
        }

        public IStockRangePredictor TrainFutures(string modelName, int numSamples)
        {
            //var datasetService = GetMarketIndicatorsDatasetService(_period, 12, _numStockSamples);
            _model = new DeepLearningStockRangePredictorModel(500, 100000, 1.0);
            //_model = new MLStockRangePredictorModelV2();
            var trainingData = new List<(FeatureVector Input, StockData Output)>();
            trainingData.AddRange(_datasetService.GetTrainingData("DIA", null, true, numSamples));
            trainingData.AddRange(_datasetService.GetTrainingData("QQQ", null, true, numSamples));
            trainingData.AddRange(_datasetService.GetTrainingData("SPY", null, true, numSamples));

            //trainingData.AddRange(_datasetService.GetTrainingData("$RUT.X", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$SPX.X", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$NDX.X", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$DJI", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1UTI", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1CYC", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1FIN", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1HCR", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1IDU", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1ENE", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1TEC", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1BSC", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1NCY", null, true, numSamples));
            //trainingData.AddRange(_datasetService.GetTrainingData("$A1TSL", null, true, numSamples));

            var averageGain = (double)trainingData.Where(x => x.Output.PercentChangeFromPreviousClose > 0.0m)
              .Average(x => Math.Abs(x.Output.PercentChangeFromPreviousClose));
            var averageLoss = (double)trainingData.Where(x => x.Output.PercentChangeFromPreviousClose <= 0.0m)
                .Average(x => Math.Abs(x.Output.PercentChangeFromPreviousClose));

            _model.Train(trainingData, 0.25, new BernoulliPercentChange(averageGain, averageLoss));

            _model.Save(modelName);

            return _model;
        }

        public IStockRangePredictor TrainStocks(string modelName, int numSamples)
        {
           // _model = new DeepLearningStockRangePredictorModel(200, 10000, 2.0);
            _model = new MLStockRangePredictorModelV2();
            //var stockFilter = new DefaultStockFilter(
            //        maxPercentHigh: 50.0m,
            //    maxPercentLow: 50.0m,
            //    minPrice: 5.0m,
            //    maxPrice: decimal.MaxValue,
            //    minVolume: 10000.0m);

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

        private IFeatureDatasetService<FeatureVector> GetMarketIndicatorsDatasetService(
            StockDataPeriod period,
            int timeSampling = 10,
            int numStockSamples = 40,
            int kernelSize = 9,
            int signalOffset = 0,
            int predictionLength = 5)
        {
            var ameritradeClient = new TDAmeritradeApiClient("I12BJE0PV9ARIGTWWOPJGCGRWPBUJLRP");
            var stocksRepo = new TDAmeritradeStockAccessService(ameritradeClient, _stockSymbolsRepository);
            var extractor = new MultiStockFeatureExtractor(new List<IFeatureExtractor<StockData>>
            {
                //Remove the fibonacci because I believe they may not be reliable
                new SupportResistanceStockFeatureExtractor(0.02m),
                new FibonacciStockFeatureExtractor(0.02m, false),
                new MACDHistogramFeatureExtraction(20),
                new RSIFeatureExtractor(10),
                new BollingerBandFeatureExtraction(10),
                new TrendStockFeatureExtractor(10, false),
                new SimpleMovingAverageFeatureExtractor(10, false)
        });

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize, signalOffset, predictionLength);
        }

        private IFeatureDatasetService<FeatureVector> GetIndicatorFeaturesBuySellSignalDatasetService(
            StockDataPeriod period,
            int timeSampling = 10,
            int numStockSamples = 40,
            int kernelSize = 9,
            int signalOffset = 0,
            int predictionLength = 5)
        {
            var ameritradeClient = new TDAmeritradeApiClient("I12BJE0PV9ARIGTWWOPJGCGRWPBUJLRP");
            var stocksRepo = new TDAmeritradeStockAccessService(ameritradeClient, _stockSymbolsRepository);
            var extractor = new MultiStockFeatureExtractor(new List<IFeatureExtractor<StockData>>
            {
                //Remove the fibonacci because I believe they may not be reliable
                new SupportResistanceStockFeatureExtractor(),
                new FibonacciStockFeatureExtractor(),
                new MACDHistogramFeatureExtraction(20),
                new RSIFeatureExtractor(10),
                new VWAPFeatureExtraction(20),
                new BollingerBandFeatureExtraction(10),
                new TrendStockFeatureExtractor(10),
                new SimpleMovingAverageFeatureExtractor(10)
        });

            return new BuySellSignalFeatureDatasetService(extractor, stocksRepo,
                period, numStockSamples, kernelSize, signalOffset, predictionLength);
        }

    }
}
