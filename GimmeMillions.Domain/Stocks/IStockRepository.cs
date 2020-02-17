using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockRepository
    {
        IEnumerable<StockData> GetStocks(string symbol);
    }
}
