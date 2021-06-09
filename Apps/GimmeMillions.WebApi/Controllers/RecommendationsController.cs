using GimmeMillions.Domain.Authentication;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.WebApi.Controllers.Dtos.Recommendations;
using GimmeMillions.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeZoneConverter;

namespace GimmeMillions.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private IRecommendationSystemProvider _provider;
        private IUserService _userService;
        private ILogger _logger;
        public RecommendationsController(IRecommendationSystemProvider provider,
            IUserService userService, ILogger logger)
        {
            _provider = provider;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("futures")]
        public IActionResult GetFutures()
        {
            var system = _provider.GetFuturesRecommendations();
            var recommendations = new List<StockRecommendation>();
            var date = GetUpdatedDailyStockDate();

            _logger.LogInformation($"Retrieving futures for {date}");

            var dia = system.GetRecommendation(date, "DIA");
            if(dia.IsSuccess)
                recommendations.Add(dia.Value);
            var qqq = system.GetRecommendation(date, "QQQ");
            if (qqq.IsSuccess)
                recommendations.Add(qqq.Value);
            var spy = system.GetRecommendation(date, "SPY");
            if (spy.IsSuccess)
                recommendations.Add(spy.Value);
            var rut = system.GetRecommendation(date, "RUT");
            if (rut.IsSuccess)
                recommendations.Add(rut.Value);

            return Ok(recommendations.Select(x => x.ToDto()));
        }

        [HttpGet("stocks/user/{username}")]
        public IActionResult GetUserWatchlistStocks(string username)
        {
            var user = _userService.GetUser(username);
            if(user.IsFailure)
            {
                return BadRequest(user.Error);
            }

            var system = _provider.GetStocksRecommendations();
            var date = GetUpdatedDailyStockDate();

            _logger.LogInformation($"Retrieving user '{username}' watchlist for {date}");
            return Ok(system.GetRecommendations(user.Value.GetWatchlist(), date).Select(x => x.ToDto())
                .OrderByDescending(x => x.Sentiment));
        }

        [HttpGet("stocks/daily")]
        public IEnumerable<StockRecommendationDto> GetDailyStocks()
        {
            var system = _provider.GetStocksRecommendations();

            var date = GetUpdatedDailyStockDate(1);
            _logger.LogInformation($"Retrieving daily stock recommendations for {date}");

            return system.GetRecommendations(date, 0).Select(x => x.ToDto()).OrderByDescending(x => x.Sentiment);
        }

        [HttpGet("stocks/{symbol}")]
        public IActionResult GetPrediction(string symbol)
        {
            var system = _provider.GetStocksRecommendations();
            var date = GetUpdatedDailyStockDate();
            _logger.LogInformation($"Retrieving {symbol} recommendation for {date}");
            var prediction = system.GetRecommendation(date, symbol.ToUpper());
            if (prediction.IsFailure)
                return BadRequest(prediction.Error);

            return Ok(prediction.Value.ToDto());
        }

        [HttpGet("stocks/history")]
        public IActionResult GetHistory(string symbol)
        {
            var system = _provider.GetStocksRecommendations();
            _logger.LogInformation($"Retrieving {symbol} history");

            var history = system.RecommendationRepository.GetStockRecommendationHistory(system.SystemId, symbol);

            if(history.IsFailure)
            {
                return BadRequest(history.Error);
            }

            return Ok(history.Value);
        }

        [HttpDelete()]
        public IActionResult WipeAll()
        {
            var system = _provider.GetStocksRecommendations();
            system.RecommendationRepository.RemoveAll();
            return Ok();
        }

        private DateTime GetUpdatedDailyStockDate(int delayHours = 0)
        {
            //get current date in PST
            var currentDate = DateTime.UtcNow.AddHours(-8.0);
            if (currentDate.Hour < 13 + delayHours)
            {
                return currentDate;
            }
            else
            {
                //If the time is after the market close PST,
                return currentDate.AddDays(1.0);
            }

        }

    }
}
