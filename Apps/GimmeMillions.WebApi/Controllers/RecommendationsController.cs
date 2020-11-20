using GimmeMillions.Domain.Stocks;
using GimmeMillions.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private IRecommendationSystemProvider _provider;
        public RecommendationsController(IRecommendationSystemProvider provider)
        {
            _provider = provider;
        }

        [HttpGet]
        public IEnumerable<StockRecommendation> Get()
        {
            var system = _provider.GetStocksRecommendations();

            var recommendations = system.RecommendationRepository.GetStockRecommendations(system.SystemId, "DIA");

            return recommendations;
        }

        [HttpGet("futures")]
        public IEnumerable<StockRecommendation> GetFutures()
        {
            var system = _provider.GetFuturesRecommendations();
            var recommendations = system.RecommendationRepository.GetStockRecommendations("cat", "DIA").ToList();
            recommendations.AddRange(system.RecommendationRepository.GetStockRecommendations("cat", "SPY"));
            recommendations.AddRange(system.RecommendationRepository.GetStockRecommendations("cat", "QQQ"));
            return recommendations;
        }
    }
}
