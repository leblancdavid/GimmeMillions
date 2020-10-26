using System;

namespace GimmeMillions.Domain.Stocks
{
    public class StockStatistics
    {
        public DateTime Date { get; set; }
        public int LongTermLength { get; set; }
        public int ShortTermLength { get; set; }
        public decimal AverageLongTermVolume { get; set; }
        public decimal AverageShortTermVolume { get; set; }
        public decimal VolumeRatio
        {
            get
            {
                return AverageShortTermVolume / AverageLongTermVolume;
            }
        }

        public decimal AverageLongTermDayRange { get; set; }
        public decimal AverageShortTermDayRange { get; set; }
        public decimal DayRangeRatio
        {
            get
            {
                return AverageShortTermDayRange / AverageLongTermDayRange;
            }
        }

        public decimal AverageLongTermTrend { get; set; }
        public decimal AverageShortTermTrend { get; set; }
        public decimal TrendRatio
        {
            get
            {
                return AverageShortTermTrend / AverageLongTermTrend;
            }
        }
    }
}
