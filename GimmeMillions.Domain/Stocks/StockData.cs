﻿using System;

namespace GimmeMillions.Domain.Stocks
{
    public class StockData
    {
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal AdjustedClose { get; set; }
        public decimal PercentDayChange
        {
            get
            {
                return (decimal)100.0 * (Close - Open) / Open;
            }
        }

        public decimal PercentChangeHighToOpen
        {
            get
            {
                return 100.0m * (High - Open) / Open;
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

        public StockData(string symbol, DateTime date, 
            decimal open, decimal high, decimal low, decimal close, decimal adjClose)
        {
            Symbol = symbol;
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            AdjustedClose = adjClose;
            PreviousClose = open;
        }
    }
}
