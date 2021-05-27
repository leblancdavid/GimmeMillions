﻿using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.DataAccess.Stocks;
using Microsoft.Extensions.Logging;
using GimmeMillions.Domain.Stocks.Recommendations;

namespace GimmeMillions.WebApi.Services
{
    public class EgyptianMauRecommendationSystemProvider : IRecommendationSystemProvider
    {
        private IStockAccessService _stockAccessService;
        private IStockRecommendationHistoryRepository _stockRecommendationRepository;
        private ILogger _logger;
        public EgyptianMauRecommendationSystemProvider(
            IStockAccessService stockAccessService,
            IStockRecommendationHistoryRepository stockRecommendationRepository,
            ILogger logger)
        {
            _stockAccessService = stockAccessService;
            _stockRecommendationRepository = stockRecommendationRepository;
            _logger = logger;
        }

        public IStockRecommendationSystem<FeatureVector> GetCryptoRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources/Models/Futures", _logger);
        }

        public IStockRecommendationSystem<FeatureVector> GetFuturesRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources/Models/Futures", _logger);
        }

        public IStockRecommendationSystem<FeatureVector> GetStocksRecommendations()
        {
            return RecommendationSystemFactory.GetEgyptianMauRecommendationSystem(
                _stockAccessService,
                _stockRecommendationRepository,
                "Resources/Models/Stocks", _logger);
        }
    }
}

