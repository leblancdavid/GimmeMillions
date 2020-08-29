using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockHistoryRepository
    {
        IEnumerable<StockHistory> GetStocks();
        Result<StockHistory> GetStock(string symbol);

        Result AddOrUpdateStock(StockHistory stockData);
        void Delete(string symbol);
        IEnumerable<string> GetSymbols();
        DateTime GetLastUpdated(string symbol);
    }
}
