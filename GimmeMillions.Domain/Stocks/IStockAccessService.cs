using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockAccessService
    {
        IEnumerable<StockData> UpdateStocks(string symbol, DateTime startDate, DateTime endDate);
    }
}
