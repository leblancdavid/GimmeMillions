using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRepository
    {
        IEnumerable<StockData> GetStocks(string symbol, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily);
        IEnumerable<StockData> GetStocks(string symbol, DateTime start, DateTime end, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily);
        Result<StockData> GetStock(string symbol, DateTime date, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily);

        IEnumerable<string> GetSymbols();
    }
}
