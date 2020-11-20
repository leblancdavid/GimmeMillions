using GimmeMillions.Domain.Stocks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private IStockRecommendationRepository _stockRecommendationRepository;
        public RecommendationsController(IStockRecommendationRepository stockRecommendationRepository)
        {
            _stockRecommendationRepository = stockRecommendationRepository;
        }

        [HttpGet]
        public IEnumerable<StockRecommendation> Get()
        {
            var recommendations = _stockRecommendationRepository.GetStockRecommendations("cat", "DIA");

            return recommendations;
        }

        [HttpGet("futures")]
        public IEnumerable<StockRecommendation> GetFutures()
        {
            var recommendations = _stockRecommendationRepository.GetStockRecommendations("cat", "DIA").ToList();
            recommendations.AddRange(_stockRecommendationRepository.GetStockRecommendations("cat", "SPY"));
            recommendations.AddRange(_stockRecommendationRepository.GetStockRecommendations("cat", "QQQ"));
            return recommendations;
        }
    }
}
