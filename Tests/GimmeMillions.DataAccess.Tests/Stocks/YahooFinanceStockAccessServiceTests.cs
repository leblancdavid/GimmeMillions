using FluentAssertions;
using GimmeMillions.DataAccess.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.DataAccess.Tests.Stocks
{
    public class YahooFinanceStockAccessServiceTests
    {
        private readonly string _pathToStocks = "../../../../../Repository/Stocks";
        [Fact]
        public void ShouldUpdateStocks()
        {
            var repo = new StockDataRepository(_pathToStocks);
            string symbol = "F";
            var stockService = new YahooFinanceStockAccessService(repo, _pathToStocks);

            var stocks = stockService.UpdateStocks(symbol, new DateTime(2000, 1, 1), DateTime.Today);

            stocks.Count().Should().BeGreaterThan(0);
        }
    }
}
