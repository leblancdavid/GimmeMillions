using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoLive.Accounts
{
    public class CryptoPosition
    {
        public string Symbol { get; set; }
        public decimal PricePaid { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal PositionSize { get; set; }
        public decimal PositionValue
        {
            get
            {
                return PositionSize * PricePaid;
            }
        }

        public CryptoPosition()
        {
            TransactionDate = DateTime.Now;
        }

        public CryptoPosition(string symbol, decimal price, decimal size)
        {
            TransactionDate = DateTime.Now;
            Symbol = symbol;
            PricePaid = price;
            PositionSize = size;
        }

        public void Add(decimal price, decimal size)
        {
            PricePaid = ((PositionValue) + (price * size)) / (size + PositionSize);
            PositionSize += size;
        }

        public decimal Drop(decimal price, decimal size)
        {
            if(size >= PositionSize)
            {
                var profits = price * PositionSize;
                PositionSize = 0.0m;
                PricePaid = 0.0m;
                return profits;
            }
            else
            {
                PricePaid = ((PositionValue) - (price * size)) / (PositionSize - size);
                PositionSize -= size;
                return price * size;
            }
        }
    }
}
