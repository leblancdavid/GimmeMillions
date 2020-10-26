using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockStatisticsCalculator
    {
        Result<StockStatistics> Compute(IEnumerable<StockData> stockData, DateTime date);
    }
}
