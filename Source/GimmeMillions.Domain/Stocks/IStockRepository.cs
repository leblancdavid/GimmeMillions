using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRepository
    {
        IStockHistoryRepository StockHistoryRepository { get; }
        IEnumerable<StockData> GetStocks(string symbol, StockDataPeriod period);
        IEnumerable<StockData> GetStocks(string symbol, DateTime start, DateTime end, StockDataPeriod period);
        Result<StockData> GetStock(string symbol, DateTime date, StockDataPeriod period);
        IEnumerable<string> GetSymbols();
    }
}
