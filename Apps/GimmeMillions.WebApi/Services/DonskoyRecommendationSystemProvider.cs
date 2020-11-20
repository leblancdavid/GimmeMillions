using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GimmeMillions.WebApi.Services
{
    public class DonskoyRecommendationSystemProvider : IRecommendationSystemProvider
    {
        private IStockRepository _stockRepository;
        private IStockRecommendationRepository _stockRecommendationRepository;
        public DonskoyRecommendationSystemProvider(IStockRepository stockRepository,
            IStockRecommendationRepository stockRecommendationRepository)
        {
            _stockRepository = stockRepository;
            _stockRecommendationRepository = stockRecommendationRepository;
        }

        public IStockRecommendationSystem<FeatureVector> GetCryptoRecommendations()
        {
            return RecommendationSystemFactory.GetDonskoyRecommendationSystem(
                _stockRepository,
                _stockRecommendationRepository,
                "Resources\\Models\\Futures");
        }

        public IStockRecommendationSystem<FeatureVector> GetFuturesRecommendations()
        {
            return RecommendationSystemFactory.GetDonskoyRecommendationSystem(
                _stockRepository,
                _stockRecommendationRepository,
                "Resources\\Models\\Stocks");
        }

        public IStockRecommendationSystem<FeatureVector> GetStocksRecommendations()
        {
            return RecommendationSystemFactory.GetDonskoyRecommendationSystem(
                _stockRepository,
                _stockRecommendationRepository,
                "Resources\\Models\\Futures");
        }
    }
}
