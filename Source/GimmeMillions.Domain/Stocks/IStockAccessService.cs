using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockAccessService
    {
        void UpdateFutures();
        IEnumerable<StockData> UpdateStocks(string symbol, FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily);
        IEnumerable<StockData> GetStocks(string symbol, FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily);
        IEnumerable<StockData> GetStocks(string symbol, int timePeriod);
        IEnumerable<StockData> GetStocks(FrequencyTimeframe frequencyTimeframe = FrequencyTimeframe.Daily);
        IEnumerable<string> GetSymbols();
    }
}
