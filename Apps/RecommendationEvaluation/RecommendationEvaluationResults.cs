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
        public StockStatistics Statistics { get; private set; }
        public RecommendationEvaluationResults(StockRecommendation stockRecommendation, StockStatistics stockStatistics)
        {
            StockRecommendation = stockRecommendation;
            Statistics = stockStatistics;
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
