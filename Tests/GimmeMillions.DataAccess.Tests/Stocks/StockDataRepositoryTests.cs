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
    public class StockDataRepositoryTests
    {
        private readonly string _pathToStocks = "../../../../Repository/Stocks";
        [Fact]
        public void ShouldGetStocks()
        {
            var repo = new StockDataRepository(_pathToStocks);

            var stocks = repo.GetStocks("IWM");

            stocks.Count().Should().BeGreaterThan(0);
        }

    }
}
