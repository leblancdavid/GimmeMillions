using CryptoLive.Accounts;
using CryptoLive.Notification;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CryptoLive.Tests
{
    public class SimulatedCryptoAccountManagerTests
    {
        [Fact]
        public void TestingBuyAndSellNotifications()
        {
            var manager = new SimulationCryptoAccountManager();
            for(int i = 0; i < 10; ++i)
            {
                manager.Notify(new CryptoEventNotification(
                   new StockData("TEST",
                   DateTime.UtcNow,
                   10.0m, 10.0m, 10.0m, 10.0m, 10.0m, 10.0m, 10.0m),
                   95.0));

                manager.Notify(new CryptoEventNotification(
                   new StockData("TEST",
                   DateTime.UtcNow,
                   20.0m, 20.0m, 20.0m, 20.0m, 20.0m, 20.0m, 20.0m),
                   5.0));

            }
            
        }

        [Fact]
        public void SaveLoadTest()
        {
            var manager = new SimulationCryptoAccountManager();
            manager.Notify(new CryptoEventNotification(
                   new StockData("TEST1",
                   DateTime.UtcNow,
                   10.0m, 10.0m, 10.0m, 10.0m, 10.0m, 10.0m, 10.0m),
                   95.0));
            manager.Notify(new CryptoEventNotification(
                   new StockData("TEST2",
                   DateTime.UtcNow,
                   20.0m, 20.0m, 20.0m, 20.0m, 20.0m, 20.0m, 20.0m),
                   95.0));

            manager.SaveAccount("TestAccount.json");
            manager.LoadAccount("TestAccount.json");

            Assert.True(manager.Account.CurrentValue >= 990.0m);
        }
    }
}
