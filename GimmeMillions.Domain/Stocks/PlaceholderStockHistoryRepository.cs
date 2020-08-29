using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class PlaceholderStockHistoryRepository : IStockHistoryRepository
    {
        public Result AddOrUpdateStock(StockHistory stockData)
        {
            return Result.Ok();
        }

        public void Delete(string symbol)
        {
        }

        public DateTime GetLastUpdated(string symbol)
        {
            return new DateTime();
        }

        public Result<StockHistory> GetStock(string symbol)
        {
            return Result.Failure<StockHistory>("Just the placeholder implementation");
        }

        public IEnumerable<StockHistory> GetStocks()
        {
            return new List<StockHistory>();
        }

        public IEnumerable<string> GetSymbols()
        {
            return new List<string>();
        }
    }
}
