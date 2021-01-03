﻿using GimmeMillions.Domain.Authentication;
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
        private IUserService _userService;
        public RecommendationsController(IRecommendationSystemProvider provider,
            IUserService userService)
        {
            _provider = provider;
            _userService = userService;
        }

        [HttpGet("futures")]
        public IActionResult GetFutures()
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

            return Ok(recommendations);
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
            return Ok(system.GetRecommendations(user.Value.GetWatchlist(), GetUpdatedDailyStockDate())
                .OrderByDescending(x => x.Sentiment));
        }

        [HttpGet("stocks/daily")]
        public IEnumerable<StockRecommendation> GetDailyStocks()
        {
            var system = _provider.GetStocksRecommendations();
            return system.GetRecommendations(GetUpdatedDailyStockDate(1), 0).OrderByDescending(x => x.Sentiment);
        }

        [HttpGet("stocks/{symbol}")]
        public IActionResult GetPrediction(string symbol)
        {
            var system = _provider.GetStocksRecommendations();
            var date = GetUpdatedDailyStockDate();
            var prediction = system.GetRecommendation(date, symbol.ToUpper());
            if (prediction.IsFailure)
                return BadRequest(prediction.Error);

            return Ok(prediction.Value);
        }

        private DateTime GetUpdatedDailyStockDate(int delayHours = 0)
        {
            var newDateTime = DateTime.Now;
            if (newDateTime.Hour < 13 + delayHours)
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
