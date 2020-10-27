﻿using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRecommendationRepository
    {
        Result AddRecommendation(StockRecommendation recommendation);
        Result UpdateRecommendation(StockRecommendation recommendation);
        Result<StockRecommendation> GetStockRecommendation(string systemId, string symbol, DateTime dateTime);
        IEnumerable<StockRecommendation> GetStockRecommendations(string systemId, string symbol);
        IEnumerable<StockRecommendation> GetStockRecommendations(string systemId, DateTime dateTime);
    }
}
