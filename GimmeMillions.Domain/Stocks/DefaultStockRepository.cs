using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class DefaultStockRepository : IStockRepository
    {
        public Result AddOrUpdateStock(StockData stockData)
        {
            throw new NotImplementedException();
        }

        public Result AddOrUpdateStocks(IEnumerable<StockData> stockData, bool overwriteExisting = false)
        {
            throw new NotImplementedException();
        }

        public void Delete(string symbol)
        {
            throw new NotImplementedException();
        }

        public Result<StockData> GetStock(string symbol, DateTime date, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockData> GetStocks(string symbol, int timeLength)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockData> GetStocks(string symbol, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StockData> GetStocks(string symbol, DateTime start, DateTime end, FrequencyTimeframe timeframe = FrequencyTimeframe.Daily)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSymbols()
        {
            throw new NotImplementedException();
        }
    }
}
