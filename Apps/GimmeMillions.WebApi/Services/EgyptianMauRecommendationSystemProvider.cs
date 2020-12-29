using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.DataAccess.Stocks;

namespace GimmeMillions.WebApi.Services
{
    public class EgyptianMauRecommendationSystemProvider : IRecommendationSystemProvider
    {
        private IStockAccessService _stockAccessService;
        private IStockRecommendationRepository _stockRecommendationRepository;
        public EgyptianMauRecommendationSystemProvider(
            IStockAccessService stockAccessService,
            IStockRecommendationRepository stockRecommendationRepository)
        {
            _stockAccessService = stockAccessService;
            _stockRecommendationRepository = stockRecommendationRepository;
        }

        public IStockRecommendationSystem<FeatureVector> GetCryptoRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources/Models/Futures");
        }

        public IStockRecommendationSystem<FeatureVector> GetFuturesRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources/Models/Futures");
        }

        public IStockRecommendationSystem<FeatureVector> GetStocksRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources/Models/Stocks");
        }
    }
}

