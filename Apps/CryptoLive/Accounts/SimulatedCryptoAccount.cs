using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoLive.Accounts
{
    public class SimulatedCryptoAccount : ICryptoAccount
    {
        private List<CryptoPosition> _currentPositions = new List<CryptoPosition>();
        public IEnumerable<CryptoPosition> CurrentPositions => _currentPositions;

        public decimal StartingFunds { get; private set; }
        public decimal AvailableFunds { get; set; }
        public DateTime DateCreated { get; private set; }
        public decimal CurrentValue
        {
            get
            {
                return AvailableFunds + _currentPositions.Sum(x => x.PositionValue);
            }
        }

        public decimal PercentChange
        {
            get
            {
                return (CurrentValue - StartingFunds) / StartingFunds * 100.0m;
            }
        }

        public SimulatedCryptoAccount()
        {
            DateCreated = DateTime.Now;
            StartingFunds = 1000.0m;
            AvailableFunds = StartingFunds;
        }

        public SimulatedCryptoAccount(decimal funds)
        {
            DateCreated = DateTime.Now;
            StartingFunds = funds;
            AvailableFunds = StartingFunds;
        }

        public void Reset(decimal funds)
        {
            DateCreated = DateTime.Now;
            StartingFunds = funds;
            AvailableFunds = StartingFunds;
            _currentPositions.Clear();
        }

        public void Buy(string symbol, decimal price, decimal quantity)
        {
            var position = _currentPositions.FirstOrDefault(x => x.Symbol == symbol);
            if (position != null)
            {
                position.Add(price, quantity);
            }
            else
            {
                _currentPositions.Add(new CryptoPosition(symbol, price, quantity));
            }
            AvailableFunds -= price * quantity;
        }

        public void Sell(string symbol, decimal price, decimal quantity)
        {
            var position = _currentPositions.FirstOrDefault(x => x.Symbol == symbol);
            if (position != null)
            {
                AvailableFunds += position.Drop(price, quantity);
                if(position.PositionSize <= 0.0m)
                {
                    _currentPositions.Remove(position);
                }
            }

        }
    }
}
