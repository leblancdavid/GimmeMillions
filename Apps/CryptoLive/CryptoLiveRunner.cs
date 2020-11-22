using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CryptoLive
{
    public class CryptoLiveRunner
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
        };

        public CryptoLiveRunner(MLStockRangePredictorModel model, IFeatureDatasetService<FeatureVector> dataset)
        {
            _model = model;
            _dataset = dataset;
            _signalTable = new Dictionary<string, Queue<(double Signal, DateTime Time)>>();
            foreach (var crypto in _supportedCryptos)
            {
                _signalTable[crypto] = new Queue<(double Signal, DateTime Time)>();
                _signalTable[crypto].Enqueue((50.0, new DateTime()));
            }

            PrintTable();
        }

        public void Refresh(int maxLength = 5)
        {
            foreach(var crypto in _supportedCryptos)
            {
                UpdateTableFor(crypto, maxLength);
            }
            PrintTable();
        }

        private void UpdateTableFor(string symbol, int maxLength = 5)
        {
            var feature = _dataset.GetFeatureVector(symbol, DateTime.UtcNow, 1);
            if (feature.IsSuccess && feature.Value.Date >= _signalTable[symbol].Last().Time)
            {
                var prediction = _model.Predict(feature.Value);
                _signalTable[symbol].Enqueue((prediction.Sentiment, feature.Value.Date));
                if(_signalTable[symbol].Count > maxLength)
                {
                    _signalTable[symbol].Dequeue();
                }
            }
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
