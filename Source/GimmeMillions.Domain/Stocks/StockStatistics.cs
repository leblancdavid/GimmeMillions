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
                if (AverageLongTermVolume == 0.0m)
                    return 0.0m;
                return AverageShortTermVolume / AverageLongTermVolume;
            }
        }

        public decimal AverageLongTermDayRange { get; set; }
        public decimal AverageShortTermDayRange { get; set; }
        public decimal DayRangeRatio
        {
            get
            {
                if (AverageLongTermDayRange == 0.0m)
                    return 0.0m;
                return AverageShortTermDayRange / AverageLongTermDayRange;
            }
        }

        public decimal AverageLongTermTrend { get; set; }
        public decimal AverageShortTermTrend { get; set; }
        public decimal TrendRatio
        {
            get
            {
                if (AverageLongTermTrend == 0.0m)
                    return 0.0m;
                return AverageShortTermTrend / AverageLongTermTrend;
            }
        }
    }
}
