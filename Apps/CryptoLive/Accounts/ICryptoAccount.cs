using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoLive.Accounts
{
    public interface ICryptoAccount
    {
        IEnumerable<CryptoPosition> CurrentPositions { get; }

        decimal StartingFunds { get; }
        decimal AvailableFunds { get; }
        DateTime DateCreated { get; }
        decimal CurrentValue { get; }
        decimal PercentChange { get; }
        void Buy(string symbol, decimal price, decimal quantity);
        void Sell(string symbol, decimal price, decimal quantity, decimal fees);
    }
}
