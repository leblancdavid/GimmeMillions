using System;
using System.Text.Json.Serialization;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRecommendation
    {
        public int Id { get; private set; }
        public string SystemId { get; set; }
        public StockData LastData { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime Date { get; set; }

        [JsonIgnore]
        public string Symbol 
        { 
            get
            {
                return LastData.Symbol;
            }
        }
        public decimal Prediction { get; set; }

        [JsonIgnore]
        public decimal PredictedPriceTarget
        { 
            get
            {
                return LastData.Close * (1.0m + Prediction / 100.0m);
            }
        }
        public decimal LowPrediction { get; set; }

        [JsonIgnore]
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

        public decimal Sentiment { get; set; }
        public decimal Confidence { get; set; }
        public StockRecommendation(string systemId, decimal prediction, DateTime date, StockData lastData)
        {
            SystemId = systemId;
            DateUpdated = DateTime.Now;
            Prediction = prediction;
            Date = date;
            LastData = lastData;
        }

        public StockRecommendation(string systemId,
            decimal highPrediction, decimal lowPrediction, decimal sentiment, decimal confidence, 
            DateTime date, StockData lastData)
        {
            SystemId = systemId;
            DateUpdated = DateTime.Now;
            Prediction = highPrediction;
            LowPrediction = lowPrediction;
            Sentiment = sentiment;
            Confidence = confidence;
            Date = date;
            LastData = lastData;
        }

        public StockRecommendation()
        {

        }
    }
}
