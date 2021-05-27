using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.Domain.Stocks
{
    public interface IStockSymbolsRepository
    {
        IEnumerable<string> GetStockSymbols();
    }
}
