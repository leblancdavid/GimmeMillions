using CryptoLive.Notification;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CryptoLive
{
    public class CryptoRealtimeScanner : ICryptoRealtimeScanner
    {
        private MLStockRangePredictorModel _model;
        private IFeatureDatasetService<FeatureVector> _dataset;
        private Dictionary<string, Queue<(double Signal, DateTime Time)>> _signalTable;
        private List<string> _supportedCryptos = new List<string>()
        {
            "BTC-USD",
            "ETH-USD",
            "LTC-USD",
            "XRP-USD",
            "BCH-USD",
            "EOS-USD",
            "DASH-USD",
            "OXT-USD",
            "MKR-USD",
            "XLM-USD",
            "LINK-USD",
            "ATOM-USD",
            "ETC-USD",
            "XTZ-USD",
            "REP-USD",
            "DAI-USD",
            "KNC-USD",
            "OMG-USD",
            "ZRX-USD",
            "ALGO-USD",
            "BAND-USD",
            "LRC-USD",
            "YFI-USD",
            "UNI-USD",
            "REN-USD",
            "BAL-USD",
        };
        private double SELL_SIGNAL_THRESHOLD = 10.0;
        private double BUY_SIGNAL_THRESHOLD = 90.0;
        private ICryptoEventNotifier _notifier;
        public ICryptoEventNotifier Notifier => _notifier;

        public CryptoRealtimeScanner(MLStockRangePredictorModel model, 
            IFeatureDatasetService<FeatureVector> dataset,
            ICryptoEventNotifier notifier,
            double buyThreshold = 90.0, double sellThreshold = 10.0)
        {
            _model = model;
            _dataset = dataset;
            _notifier = notifier;
            BUY_SIGNAL_THRESHOLD = buyThreshold;
            SELL_SIGNAL_THRESHOLD = sellThreshold;
            _signalTable = new Dictionary<string, Queue<(double Signal, DateTime Time)>>();
            foreach (var crypto in _supportedCryptos)
            {
                _signalTable[crypto] = new Queue<(double Signal, DateTime Time)>();
                _signalTable[crypto].Enqueue((50.0, new DateTime()));
            }

            PrintTable();
        }

        public IEnumerable<CryptoEventNotification> Scan()
        {
            int length = 5;
            var results = new List<CryptoEventNotification>();
            foreach(var crypto in _supportedCryptos)
            {
                var result = UpdateTableFor(crypto, length);
                if (result == null)
                    continue;

                results.Add(result);
                if (_notifier != null && result.IsBuySignal(BUY_SIGNAL_THRESHOLD))
                    _notifier.Notify(result);
                if (_notifier != null && result.IsSellSignal(SELL_SIGNAL_THRESHOLD))
                    _notifier.Notify(result);
            }
            PrintTable();
            return results;
        }

        private CryptoEventNotification UpdateTableFor(string symbol, int maxLength = 5)
        {
            StockData last = null;
            var feature = _dataset.GetFeatureVector(symbol, out last, 1);
            if (feature.IsSuccess && feature.Value.Date >= _signalTable[symbol].Last().Time)
            {
                var prediction = _model.Predict(feature.Value);
                _signalTable[symbol].Enqueue((prediction.Sentiment, feature.Value.Date));
                if (_signalTable[symbol].Count > maxLength)
                {
                    _signalTable[symbol].Dequeue();
                }

                if(_signalTable[symbol].Count < maxLength)
                    return new CryptoEventNotification(last, _signalTable[symbol].Last().Signal);

                var signal = (_signalTable[symbol].ElementAt(maxLength - 1).Signal + _signalTable[symbol].ElementAt(maxLength - 2).Signal) / 2.0;
                return new CryptoEventNotification(last, signal);
            }

            return null;

        }

        private void PrintTable()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ResetColor();
            foreach (var crypto in _supportedCryptos)
            {
                string line = $"{crypto.Replace("-USD", "")}";
                var color = GetColorFromSignal(_signalTable[crypto].Last().Signal);
                foreach(var signal in _signalTable[crypto].Reverse())
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
