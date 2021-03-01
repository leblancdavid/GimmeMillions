﻿using GimmeMillions.Domain.Features;
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
        private Dictionary<string, Queue<(double Signal, DateTime Time)>> _signalTable;
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
            _signalTable = new Dictionary<string, Queue<(double Signal, DateTime Time)>>();
            foreach (var symbol in _symbols)
            {
                _signalTable[symbol] = new Queue<(double Signal, DateTime Time)>();
                _signalTable[symbol].Enqueue((50.0, new DateTime()));
            }

            PrintTable();
        }

        public IEnumerable<BuySellNotification> Scan()
        {
            int length = 5;
            var results = new List<BuySellNotification>();
            foreach (var symbol in _symbols)
            {
                var result = UpdateTableFor(symbol, length);
                if (result == null)
                    continue;

                results.Add(result);
                //if (_notifier != null && result.IsBuySignal(BUY_SIGNAL_THRESHOLD))
                //    _notifier.Notify(result);
                //if (_notifier != null && result.IsSellSignal(SELL_SIGNAL_THRESHOLD))
                //    _notifier.Notify(result);
            }
            PrintTable();
            return results;
        }

        private BuySellNotification UpdateTableFor(string symbol, int maxLength = 5)
        {
            StockData last = null;
            int historyLength = 100;
            var feature = _dataset.GetFeatureVector(symbol, out last, historyLength);
            if (feature.IsSuccess && feature.Value.Date >= _signalTable[symbol].Last().Time)
            {
                var prediction = _model.Predict(feature.Value);
                if (prediction.Sentiment != _signalTable[symbol].Last().Signal)
                {
                    _signalTable[symbol].Enqueue((prediction.Sentiment, feature.Value.Date));
                    if (_signalTable[symbol].Count > maxLength)
                    {
                        _signalTable[symbol].Dequeue();
                    }
                    return new BuySellNotification(last, prediction.Sentiment);
                }
            }

            return null;

        }

        private void PrintTable()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ResetColor();
            foreach (var symbol in _symbols)
            {
                string line = symbol;
                var color = GetColorFromSignal(_signalTable[symbol].Last().Signal);
                foreach (var signal in _signalTable[symbol].Reverse())
                {
                    string s = String.Format("{0:F2}", signal.Signal);
                    line += $"\t{s}%";
                }
                Console.ForegroundColor = color;
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
