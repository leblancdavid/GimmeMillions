﻿using System;
using System.Collections.Generic;

namespace GimmeMillions.WebApi.Controllers.Dtos.Recommendations
{
    public class StockRecommendationHistoryDto
    {
        public string SystemId { get; set; }
        public string Symbol { get; set; }
        public List<StockRecommendationDto> HistoricalData { get; set; }
        public DateTime LastUpdated { get; set; }
        public StockRecommendationDto LastRecommendation { get; set; }

        public StockRecommendationHistoryDto()
        {
            HistoricalData = new List<StockRecommendationDto>();
        }
    }
}
