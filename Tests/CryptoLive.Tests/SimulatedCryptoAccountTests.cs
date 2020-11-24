using CryptoLive.Accounts;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CryptoLive.Tests
{
    public class SimulatedCryptoAccountTests
    {
        [Fact]
        public void BuySellFunctionalityShouldWork()
        {
            var account = new SimulatedCryptoAccount(1000.0m);
            account.Buy("TEST", 10.0m, 10.0m);
            Assert.True(account.CurrentValue == 1000.0m);
            Assert.True(account.AvailableFunds == 900.0m);
            account.Sell("TEST", 20.0m, decimal.MaxValue);
            Assert.True(account.CurrentValue == 1100.0m);
            Assert.True(account.AvailableFunds == 1100.0m);
        }

        

    }
}
