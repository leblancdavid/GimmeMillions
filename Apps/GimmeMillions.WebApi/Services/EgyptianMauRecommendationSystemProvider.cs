using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.DataAccess.Stocks;

namespace GimmeMillions.WebApi.Services
{
    public class EgyptianMauRecommendationSystemProvider : IRecommendationSystemProvider
    {
        private IStockRepository _stockRepository;
        private IStockAccessService _stockAccessService;
        private IStockRecommendationRepository _stockRecommendationRepository;
        public EgyptianMauRecommendationSystemProvider(IStockRepository stockRepository,
            IStockAccessService stockAccessService,
            IStockRecommendationRepository stockRecommendationRepository)
        {
            _stockRepository = stockRepository;
            _stockAccessService = stockAccessService;
            _stockRecommendationRepository = stockRecommendationRepository;
        }

        public IStockRecommendationSystem<FeatureVector> GetCryptoRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources\\Models\\Futures");
        }

        public IStockRecommendationSystem<FeatureVector> GetFuturesRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources\\Models\\Futures");
        }

        public IStockRecommendationSystem<FeatureVector> GetStocksRecommendations()
        {
            return RecommendationSystemFactory.GetCatRecommendationSystem(
                _stockRepository,
                _stockRecommendationRepository,
                "Resources\\Models\\CatSmallCaps");
        }
    }
}
}
