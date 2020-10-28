using System;

namespace GimmeMillions.Domain.Stocks
{
    public class StockData
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal AdjustedClose { get; set; }
        public decimal Volume { get; set; }
        public decimal PercentDayChange
        {
            get
            {
                return (decimal)100.0 * (Close - Open) / Open;
            }
        }

        public decimal PercentDayRange
        {
            get
            {
                if (Low == 0.0m)
                    return 0.0m;

                return (decimal)100.0 * (High - Low) / Low;
            }
        }

        public decimal AveragePercentDayRange { get; set; }

        public decimal PercentChangeHighToOpen
        {
            get
            {
                return 100.0m * (High - Open) / Open;
            }
        }

        public decimal PercentHighToLow
        {
            get
            {
                return 100.0m * (High - Low) / Low;
            }
        }

        public decimal PreviousClose { get; set; }

        public decimal PercentChangeHighToPreviousClose
        {
            get
            {
                return 100.0m * (High - PreviousClose) / PreviousClose;
            }
        }

        public decimal PercentChangeLowToPreviousClose
        {
            get
            {
                return 100.0m * (Low - PreviousClose) / PreviousClose;
            }
        }

        public decimal PercentChangeOpenToPreviousClose
        {
            get
            {
                return 100.0m * (Open - PreviousClose) / PreviousClose;
            }
        }

        public decimal PercentChangeFromPreviousClose
        {
            get
            {
                return 100.0m * (Close - PreviousClose) / PreviousClose;
            }
        }

        public decimal AverageTrend
        {
            get
            {
                return (PercentChangeHighToPreviousClose +
                    PercentChangeLowToPreviousClose +
                    PercentChangeOpenToPreviousClose +
                    PercentChangeFromPreviousClose) / 4.0m;
            }
        }

        public decimal TopWickPercent
        {
            get
            {
                if(PercentDayChange > 0.0m)
                {
                    return 100.0m * (High - Close) / Open;
                }
                else
                {
                    return 100.0m * (High - Open) / Open;
                }
            }
        }

        public decimal BottomWickPercent
        {
            get
            {
                if (PercentDayChange > 0.0m)
                {
                    return 100.0m * (Open - Low) / Open;
                }
                else
                {
                    return 100.0m * (Close - Low) / Open;
                }
            }
        }

        public decimal CMF
        {
            get
            {
                if(High == Low)
                {
                    return 0.0m;
                }
                return ((Close - Low) - (High - Close)) / (High - Low);
            }
        }
        public StockData(string symbol, DateTime date, 
            decimal open, decimal high, decimal low, decimal close, decimal adjClose, decimal volume)
        {
            Symbol = symbol;
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            AdjustedClose = adjClose;
            PreviousClose = open;
            Volume = volume;
            AveragePercentDayRange = PercentDayRange;
        }

        public StockData(string symbol, DateTime date,
           decimal open, decimal high, decimal low, decimal close, decimal adjClose, decimal volume,
           decimal previousClose)
        {
            Symbol = symbol;
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            AdjustedClose = adjClose;
            PreviousClose = previousClose;
            Volume = volume;
            AveragePercentDayRange = PercentDayRange;
        }

        public StockData(int id, string symbol, DateTime date,
           decimal open, decimal high, decimal low, decimal close, decimal adjClose, decimal volume,
           decimal previousClose)
        {
            Id = id;
            Symbol = symbol;
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            AdjustedClose = adjClose;
            PreviousClose = previousClose;
            Volume = volume;
            AveragePercentDayRange = PercentDayRange;
        }

        private StockData() { } //default constructor
    }
}
