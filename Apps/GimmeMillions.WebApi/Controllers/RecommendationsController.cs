using GimmeMillions.Domain.Stocks;
using GimmeMillions.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.WebApi.Controllers
{
    [Authorize]
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
            var date = GetUpdatedDailyStockDate();
            var dia = system.GetRecommendation(date, "DIA");
            if(dia.IsSuccess)
                recommendations.Add(dia.Value);
            var qqq = system.GetRecommendation(date, "QQQ");
            if (qqq.IsSuccess)
                recommendations.Add(qqq.Value);
            var spy = system.GetRecommendation(date, "SPY");
            if (spy.IsSuccess)
                recommendations.Add(spy.Value);

            return recommendations;
        }

        private DateTime GetUpdatedDailyStockDate()
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
            var newDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
            if (newDateTime.Hour < 13)
            {
                return DateTime.Today;
            }
            else
            {
                //If the time is after the market close PST,
                return DateTime.Today.AddDays(1.0);
            }

        }

    }
}
