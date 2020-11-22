using Colorful;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

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
        };

        public CryptoLiveRunner(MLStockRangePredictorModel model, IFeatureDatasetService<FeatureVector> dataset)
        {
            _model = model;
            _dataset = dataset;
            _signalTable = new Dictionary<string, Queue<(double Signal, DateTime Time)>>();
            foreach (var crypto in _supportedCryptos)
            {
                _signalTable[crypto] = new Queue<(double Signal, DateTime Time)>();
                _signalTable[crypto].Enqueue((50.0, DateTime.UtcNow));
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
            Colorful.Console.Clear();
            foreach (var crypto in _supportedCryptos)
            {
                Colorful.Console.Write($"{crypto}");
                foreach(var signal in _signalTable[crypto])
                {
                    Colorful.Console.Write($"\t{signal.Signal}%", GetColorFromSignal(signal.Signal));
                }
                Colorful.Console.Write("\n");
            }
        }

        private Color GetColorFromSignal(double signal)
        {
            if(signal > 50.0)
            {
                return Color.FromArgb((int)((signal - 50.0) * 4.0), 200, 50);
            }

            return Color.FromArgb(200, (int)((signal) * 4.0), 50);
        }
    }
}
