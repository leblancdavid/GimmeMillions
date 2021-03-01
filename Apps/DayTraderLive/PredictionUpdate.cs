using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;
using System.Text;

namespace DayTraderLive
{
    public class PredictionUpdate
    {
        public StockRangePrediction CurrentPrediction { get; private set; }
        public StockRangePrediction LastPrediction { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public double SignalChange
        {
            get
            {
                return CurrentPrediction.Sentiment - LastPrediction.Sentiment;
            }
        }

        public PredictionUpdate()
        {
            CurrentPrediction = new StockRangePrediction();
            Update(CurrentPrediction);
        }

        public PredictionUpdate(StockRangePrediction prediction)
        {
            CurrentPrediction = prediction;
            Update(prediction);
        }

        public void Update(StockRangePrediction prediction)
        {
            LastUpdated = DateTime.Now;
            LastPrediction = CurrentPrediction;
            CurrentPrediction = prediction;
        }
    }
}
