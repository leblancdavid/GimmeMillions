using System;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRecommendation
    {
        public int Id { get; private set; }
        public string SystemId { get; private set; }
        public StockData LastData { get; private set; }
        public DateTime DateUpdated { get; private set; }
        public DateTime Date { get; private set; }
        public string Symbol 
        { 
            get
            {
                return LastData.Symbol;
            }
        }
        public decimal Prediction { get; private set; }
        public decimal PredictedPriceTarget
        { 
            get
            {
                return LastData.Close * (1.0m + Prediction / 100.0m);
            }
        }
        public decimal LowPrediction { get; private set; }
        public decimal PredictedLowTarget
        {
            get
            {
                return LastData.Close * (1.0m + LowPrediction / 100.0m);
            }
        }
        public decimal PreviousClose
        {
            get
            {
                return LastData.Close;
            }
        }

        public decimal Sentiment { get; private set; }
        public StockRecommendation(string systemId, decimal prediction, DateTime date, StockData lastData)
        {
            SystemId = systemId;
            DateUpdated = DateTime.Now;
            Prediction = prediction;
            Date = date;
            LastData = lastData;
        }

        public StockRecommendation(string systemId,
            decimal highPrediction, decimal lowPrediction, decimal sentiment, DateTime date, StockData lastData)
        {
            SystemId = systemId;
            DateUpdated = DateTime.Now;
            Prediction = highPrediction;
            LowPrediction = lowPrediction;
            Sentiment = sentiment;
            Date = date;
            LastData = lastData;
        }

        public StockRecommendation()
        {

        }
    }
}
