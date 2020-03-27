using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRepository
    {
        IEnumerable<StockData> GetStocks(string symbol);
        IEnumerable<StockData> GetStocks(string symbol, DateTime start, DateTime end);
        Result<StockData> GetStock(string symbol, DateTime date);

        IEnumerable<string> GetSymbols();
    }
}
