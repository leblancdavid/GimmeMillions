using GimmeMillions.Domain.ML;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace DayTraderLive
{
    public class PredictionUpdate
    {
        public StockRangePrediction CurrentPrediction { get; private set; }
        public StockRangePrediction LastPrediction { get; private set; }
        public StockData LastCandle { get; private set; }
        public double SignalChange
        {
            get
            {
                return CurrentPrediction.Sentiment - LastPrediction.Sentiment;
            }
        }

        public decimal HighTarget
        {
            get
            {
                return LastCandle.Close * (decimal)(1.0 + CurrentPrediction.PredictedHigh / 100.0);
            }
        }
        public decimal LowTarget
        {
            get
            {
                return LastCandle.Close * (decimal)(1.0 + CurrentPrediction.PredictedLow / 100.0);
            }
        }

        public PredictionUpdate()
        {
            CurrentPrediction = new StockRangePrediction();
            LastCandle = new StockData("", new DateTime(), 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m);
            Update(CurrentPrediction, LastCandle);
        }

        public PredictionUpdate(StockRangePrediction prediction)
        {
            CurrentPrediction = prediction;
            Update(prediction, new StockData("", new DateTime(), 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m));
        }

        public void Update(StockRangePrediction prediction, StockData last)
        {
            LastCandle = last;
            LastPrediction = CurrentPrediction;
            CurrentPrediction = prediction;
        }
    }
}
