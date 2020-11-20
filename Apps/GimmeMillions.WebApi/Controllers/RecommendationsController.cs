using GimmeMillions.Domain.Stocks;
using GimmeMillions.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var recommendations = new List<StockRecommendation>();
            recommendations.Add(system.GetRecommendation(DateTime.Today, "DIA").Value);
            recommendations.Add(system.GetRecommendation(DateTime.Today, "QQQ").Value);
            recommendations.Add(system.GetRecommendation(DateTime.Today, "SPY").Value);
            return recommendations;
        }

        [HttpPut("futures")]
        public IActionResult UpdateFutures()
        {
            var system = _provider.GetFuturesRecommendations();
            var recommendations = new List<StockRecommendation>(); 
            var stockList = new List<string>()
            {
                "DIA", "QQQ", "SPY"
            };
            recommendations.AddRange(system.RunRecommendationsFor(stockList, DateTime.Today, null));
            return Ok(recommendations);
        }
    }
}
