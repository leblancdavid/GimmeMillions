using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.WebApi.Controllers.Dtos.Recommendations
{
    public class StockRecommendationDto
    {
        public string SystemId { get; set; }
        public StockDataDto LastData { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime Date { get; set; }
        public string Symbol { get; set; }
        public decimal Prediction { get; set; }
        public decimal PredictedPriceTarget { get; set; }
        public decimal LowPrediction { get; set; }
        public decimal PredictedLowTarget { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal Sentiment { get; set; }
        public decimal Confidence { get; set; }
    }
}
