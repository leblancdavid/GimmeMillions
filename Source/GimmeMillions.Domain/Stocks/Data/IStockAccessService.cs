using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockAccessService
    {
        IEnumerable<StockData> UpdateStocks(string symbol, StockDataPeriod period, int limit = -1);
        IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period, int limit = -1);
        IEnumerable<string> GetSymbols();
    }
}
