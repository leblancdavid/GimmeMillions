using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Stocks
{
    public enum StockDataPeriod
    {
        Week = 432000,
        Day = 86400,
        SixHour = 21600,
        Hour = 3600,
        FifteenMinute = 900,
        FiveMinute = 300,
        Minute = 60
    };

    public class StockData
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public StockDataPeriod Period { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal AdjustedClose { get; set; }
        public decimal Volume { get; set; }
        public decimal PercentPeriodChange
        {
            get
            {
                if (Open == 0.0m)
                    return 0.0m;
                return (decimal)100.0 * (Close - Open) / Open;
            }
        }

        public decimal PercentPeriodRange
        {
            get
            {
                if (Low == 0.0m)
                    return 0.0m;

                return (decimal)100.0 * (High - Low) / Low;
            }
        }

        public decimal AveragePercentPeriodRange { get; set; }

        public decimal PercentChangeHighToOpen
        {
            get
            {
                if (Open == 0.0m)
                    return 0.0m;
                return 100.0m * (High - Open) / Open;
            }
        }

        public decimal PercentHighToLow
        {
            get
            {
                if (Low == 0.0m)
                    return 0.0m;
                return 100.0m * (High - Low) / Low;
            }
        }

        public decimal PreviousClose { get; set; }

        public decimal PercentChangeHighToPreviousClose
        {
            get
            {
                if (PreviousClose == 0.0m)
                    return 0.0m;
                return 100.0m * (High - PreviousClose) / PreviousClose;
            }
        }

        public decimal PercentChangeLowToPreviousClose
        {
            get
            {
                if (PreviousClose == 0.0m)
                    return 0.0m;
                return 100.0m * (Low - PreviousClose) / PreviousClose;
            }
        }

        public decimal PercentChangeOpenToPreviousClose
        {
            get
            {
                if (PreviousClose == 0.0m)
                    return 0.0m;
                return 100.0m * (Open - PreviousClose) / PreviousClose;
            }
        }

        public decimal PercentChangeFromPreviousClose
        {
            get
            {
                if (PreviousClose == 0.0m)
                    return 0.0m;
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

        public decimal Average
        {
            get
            {
                return (High + Low + Open + Close) / 4.0m;
            }
        }

        public decimal TopWickPercent
        {
            get
            {
                if (Open == 0.0m)
                {
                    return 0.0m;
                }

                if (PercentPeriodChange > 0.0m)
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
                if (Open == 0.0m)
                {
                    return 0.0m;
                }

                if (PercentPeriodChange > 0.0m)
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
                if (High == Low)
                {
                    return 0.0m;
                }
                return ((Close - Low) - (High - Close)) / (High - Low);
            }
        }

        public decimal PriceNormalizedVolume
        {
            get
            {
                return Math.Abs(Average * Volume);
            }
        }

        public decimal Signal { get; set; }

        public StockData(string symbol, DateTime date, StockDataPeriod period,
            decimal open, decimal high, decimal low, decimal close, decimal adjClose, decimal volume)
        {
            Symbol = symbol;
            Date = date;
            Period = period;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            AdjustedClose = adjClose;
            PreviousClose = open;
            Volume = volume + 1; //ensure volume is always at least 1
            AveragePercentPeriodRange = PercentPeriodRange;
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
            Volume = volume + 1; //ensure volume is always at least 1
            AveragePercentPeriodRange = PercentPeriodRange;
            Period = StockDataPeriod.Day;
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
            Volume = volume + 1; //ensure volume is always at least 1
            AveragePercentPeriodRange = PercentPeriodRange;
            Period = StockDataPeriod.Day;
        }

        public void ApplyScaling(decimal scale)
        {
            Open *= scale;
            High *= scale;
            Low *= scale;
            Close *= scale;
            AdjustedClose *= scale;
            PreviousClose *= scale;
        }

        public void Invert()
        {
            Open *= -1.0m;
            var newHigh = -1.0m * Low;
            Low *= -1.0m * High;
            High *= newHigh;
            Close *= -1.0m;
            AdjustedClose *= -1.0m;
            PreviousClose *= -1.0m;
        }

        private StockData() { } //default constructor

        public static StockData Combine(IEnumerable<StockData> stockDatas)
        {
            decimal high = stockDatas.Max(x => x.High);
            decimal low = stockDatas.Min(x => x.Low);
            decimal volume = stockDatas.Sum(x => x.Volume) + 1; //ensure volume is always at least 1
            DateTime date = stockDatas.Min(x => x.Date);
            DateTime endDate = stockDatas.Max(x => x.Date);

            var first = stockDatas.First(x => x.Date == date);
            var last = stockDatas.First(x => x.Date == endDate);
            return new StockData(first.Symbol, date, first.Open, high, low, last.Close, last.AdjustedClose, volume, first.PreviousClose);
        }
    }
}