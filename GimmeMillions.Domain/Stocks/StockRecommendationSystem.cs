using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class StockRecommendationSystem : IStockRecommendationSystem
    {
        private List<IStockPredictionModel> _models;
        private IFeatureDatasetService _featureDatasetService;

        public StockRecommendationSystem(IFeatureDatasetService featureDatasetService)
        {
            _models = new List<IStockPredictionModel>();
            _featureDatasetService = featureDatasetService;
        }

        public IEnumerable<StockRecommendation> GetRecommendations(DateTime date)
        {
            var recommendations = new List<StockRecommendation>();
            var feature = _featureDatasetService.GetData(date);
            if(feature.IsFailure)
            {
                return recommendations;
            }

            var totalScore = 0.0;
            foreach(var model in _models)
            {
                var result = model.Predict(feature.Value);
                if(result.PredictedLabel)
                {
                    totalScore += result.Score;
                    recommendations.Add(new StockRecommendation(model.StockSymbol, result));
                }
            }

            foreach(var recommendation in recommendations)
            {
                recommendation.RecommendedInvestmentPercentage = recommendation.Prediction.Score / totalScore;
            }

            return recommendations.OrderByDescending(x => x.RecommendedInvestmentPercentage);
        }

        public IEnumerable<StockRecommendation> GetRecommendationsForToday()
        {
            return GetRecommendations(DateTime.Today);
        }
    }
}
