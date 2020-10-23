using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendationEvaluation
{
    public class RecommendationEvaluationResults
    {
        public StockRecommendation StockRecommendation { get; private set; }
        public RecommendationEvaluationResults(StockRecommendation stockRecommendation)
        {
            StockRecommendation = stockRecommendation;
            DaysToHitTarget = 0;
        }

        public int DaysToHitTarget { get; set; }
        public decimal HighOverPeriod { get; set; }
        public decimal PredictionToHighPercent
        {
            get
            {
                return (HighOverPeriod - StockRecommendation.PredictedPriceTarget) / StockRecommendation.PredictedPriceTarget;
            }
        }


    }
}
