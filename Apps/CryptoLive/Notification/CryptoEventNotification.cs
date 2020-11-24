using GimmeMillions.Domain.Stocks;
using System;

namespace CryptoLive.Notification
{
    public class CryptoEventNotification
    {
        public StockData LastBar { get; private set; }

        public double Signal { get; private set; }
        public DateTime Time
        {
            get
            {
                return LastBar.Date;
            }
        }
        public string CryptoSymbol
        {
            get
            {
                return LastBar.Symbol;
            }
        }

        public CryptoEventNotification(StockData lastBar, double signal)
        {
            LastBar = lastBar;
            Signal = signal;
        }

        public bool IsBuySignal(double threshold = 50.0)
        {
            if(Signal > threshold)
            {
                return true;
            }
            return false;
        }

        public bool IsSellSignal(double threshold = 50.0)
        {
            if(Signal < threshold)
            {
                return true;
            }
            return false;
        }

    }
}
