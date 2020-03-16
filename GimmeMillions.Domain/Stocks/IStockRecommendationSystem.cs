using CSharpFunctionalExtensions;
using GimmeMillions.Domain.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRecommendationSystem
    {
        Result LoadConfiguration(string configurationFile);
        Result SaveConfiguration(string configurationFile);
        void AddModel(IStockPredictionModel stockPredictionModel);
        Result RetrainModels(DateTime startTime, DateTime endTime);
        IEnumerable<StockRecommendation> GetRecommendationsForToday();
        IEnumerable<StockRecommendation> GetRecommendations(DateTime date);
    }
}
