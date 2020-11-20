using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.WebApi.Services
{
    public interface IRecommendationSystemProvider
    {
        IStockRecommendationSystem<FeatureVector> GetFuturesRecommendations();
        IStockRecommendationSystem<FeatureVector> GetStocksRecommendations();
        IStockRecommendationSystem<FeatureVector> GetCryptoRecommendations();

    }
}
