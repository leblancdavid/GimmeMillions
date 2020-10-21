using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRecommendation
    {
        public int Id { get; private set; }
        public DateTime Date { get; private set; }
        public string Symbol { get; private set; }
        public decimal Prediction { get; private set; }
        public decimal PreviousClose { get; private set; }
        public decimal PredictedPriceTarget { get; private set; }
        public StockRecommendation(DateTime date, string symbol, decimal prediction, decimal priceTarget, decimal previousClose)
        {
            Date = date;
            Symbol = symbol;
            Prediction = prediction;
            PredictedPriceTarget = priceTarget;
            PreviousClose = previousClose;
        }

        public StockRecommendation()
        {

        }
    }
}
