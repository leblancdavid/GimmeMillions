using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoLive.Accounts
{
    public enum CryptoTransactionStatus
    {
        Open,
        Closed
    };
    public class CryptoTransaction : CryptoPosition
    {
        public CryptoTransactionStatus Status { get; set; }
        public decimal ClosePrice { set; get; }
        public DateTime DateClosed { get; set; }
        public decimal CloseValue
        {
            get
            {
                return PositionSize * ClosePrice;
            }
        }
        public decimal Profits
        {
            get
            {
                return CloseValue - PositionValue;
            }
        }
        public decimal Gains
        {
            get
            {
                return Profits / PositionValue;
            }
        }
        public CryptoTransaction(string symbol, decimal price, decimal size)
        {
            TransactionDate = DateTime.Now;
            Symbol = symbol;
            PricePaid = price;
            PositionSize = size;
            Status = CryptoTransactionStatus.Open;
        }

        public void Close(decimal closePrice)
        {
            ClosePrice = closePrice;
            Status = CryptoTransactionStatus.Closed;
        }
    }
}
