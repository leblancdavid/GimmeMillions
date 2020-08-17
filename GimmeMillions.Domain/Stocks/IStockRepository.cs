using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRepository
    {
        IEnumerable<StockData> GetStocks(string symbol, int timeLength);
        IEnumerable<StockData> GetStocks(string symbol, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily);
        IEnumerable<StockData> GetStocks(string symbol, DateTime start, DateTime end, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily);
        Result<StockData> GetStock(string symbol, DateTime date, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily);

        Result AddOrUpdateStock(StockData stockData);
        Result AddOrUpdateStocks(IEnumerable<StockData> stockData, bool overwriteExisting = false);
        void Delete(string symbol);
        IEnumerable<string> GetSymbols();
    }
}
