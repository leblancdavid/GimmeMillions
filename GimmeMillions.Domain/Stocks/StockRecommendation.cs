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
        public string Symbol { get; private set; }
        public double RecommendedInvestmentPercentage { get; set; }

        public StockPrediction Prediction { get; private set; }

        public StockRecommendation(string symbol, StockPrediction prediction)
        {
            Symbol = symbol;
            Prediction = prediction;
            RecommendedInvestmentPercentage = 0.0;
        }
    }
}
