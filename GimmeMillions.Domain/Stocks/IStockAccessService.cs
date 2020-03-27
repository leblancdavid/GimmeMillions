using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockAccessService
    {
        IEnumerable<StockData> UpdateStocks(string symbol);
        IEnumerable<StockData> GetStocks(string symbol);
        IEnumerable<StockData> GetStocks();
        IEnumerable<string> GetSymbols();
    }
}
