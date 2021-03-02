using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DayTraderLive
{
    public class DayTradeFuturesScanner
    {
        private MLStockRangePredictorModel _model;
        private IFeatureDatasetService<FeatureVector> _dataset;
        private Dictionary<string, PredictionUpdate> _predictionTable;
        private List<string> _symbols = new List<string>()
        {
            "DIA", "SPY", "QQQ", "RUT"
        };
        private double SELL_SIGNAL_THRESHOLD = 10.0;
        private double BUY_SIGNAL_THRESHOLD = 90.0;
       
        public DayTradeFuturesScanner(MLStockRangePredictorModel model,
            IFeatureDatasetService<FeatureVector> dataset,
            double buyThreshold = 90.0, double sellThreshold = 10.0)
        {
            _model = model;
            _dataset = dataset;
            BUY_SIGNAL_THRESHOLD = buyThreshold;
            SELL_SIGNAL_THRESHOLD = sellThreshold;
            _predictionTable = new Dictionary<string, PredictionUpdate>();
            foreach (var symbol in _symbols)
            {
                _predictionTable[symbol] = new PredictionUpdate();
            }

            PrintTable();
        }

        public IEnumerable<PredictionUpdate> Scan()
        {
            int length = 5;
            var results = new List<PredictionUpdate>();
            foreach (var symbol in _symbols)
            {
                var result = UpdateTableFor(symbol, length);
                if (result == null)
                    continue;

                results.Add(result);
            }
            PrintTable();
            return results;
        }

        private PredictionUpdate UpdateTableFor(string symbol, int maxLength = 5)
        {
            StockData last = null;
            int historyLength = 100;
            var feature = _dataset.GetFeatureVector(symbol, out last, historyLength);
            if (feature.IsSuccess)
            {
                var prediction = _model.Predict(feature.Value);
                _predictionTable[symbol].Update(prediction, last);
              
                return _predictionTable[symbol];
            }

            return null;

        }

        private void PrintTable()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ResetColor();

            Console.WriteLine("Symbol\tSignal\tChange\tHigh\tTarget\tLow\tTarget\tLast Update");
            foreach (var symbol in _symbols)
            {
                string line = symbol;
                var color = GetColorFromSignal(_predictionTable[symbol].CurrentPrediction.Sentiment);
                Console.ForegroundColor = color;
                line += $"\t{String.Format("{0:F2}", _predictionTable[symbol].CurrentPrediction.Sentiment)}%";
                line += $"\t{String.Format("{0:F2}", _predictionTable[symbol].SignalChange)}%";
                line += $"\t{String.Format("{0:F2}", _predictionTable[symbol].CurrentPrediction.PredictedHigh)}%";
                line += $"\t{String.Format("{0:F2}", _predictionTable[symbol].HighTarget)}";
                line += $"\t{String.Format("{0:F2}", _predictionTable[symbol].CurrentPrediction.PredictedLow)}%";
                line += $"\t{String.Format("{0:F2}", _predictionTable[symbol].LowTarget)}";
                line += $"\t{_predictionTable[symbol].LastCandle.Date.ToString("hh:mm:ss.F")}";
                Console.WriteLine(line);
            }
        }

        private ConsoleColor GetColorFromSignal(double signal)
        {
            if (signal > 85.0)
                return ConsoleColor.DarkGreen;
            if (signal > 70.0)
                return ConsoleColor.Green;
            if (signal > 60.0)
                return ConsoleColor.Yellow;
            if (signal > 30.0)
                return ConsoleColor.DarkYellow;
            if (signal > 15.0)
                return ConsoleColor.Red;
            return ConsoleColor.DarkRed;
        }
    }
}
