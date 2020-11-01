using System;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRecommendation
    {
        public int Id { get; private set; }
        public string SystemId { get; private set; }
        public DateTime Date { get; private set; }
        public string Symbol { get; private set; }
        public decimal Prediction { get; private set; }
        public decimal PreviousClose { get; private set; }
        public decimal PredictedPriceTarget
        { 
            get
            {
                return PreviousClose * (1.0m + Prediction / 100.0m);
            }
        }
        public decimal LowPrediction { get; private set; }
        public decimal PredictedLowTarget
        {
            get
            {
                return PreviousClose * (1.0m + LowPrediction / 100.0m);
            }
        }

        public decimal Sentiment { get; private set; }
        public StockRecommendation(string systemId, DateTime date, string symbol, decimal prediction, decimal previousClose)
        {
            SystemId = systemId;
            Date = date;
            Symbol = symbol;
            Prediction = prediction;
            PreviousClose = previousClose;
        }

        public StockRecommendation(string systemId, DateTime date, string symbol, 
            decimal highPrediction, decimal lowPrediction, decimal sentiment, decimal previousClose)
        {
            SystemId = systemId;
            Date = date;
            Symbol = symbol;
            Prediction = highPrediction;
            LowPrediction = lowPrediction;
            Sentiment = sentiment;
            PreviousClose = previousClose;
        }

        public StockRecommendation()
        {

        }
    }
}
