using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class StockRiseDataFeature
    {
        public double[] News { get; set; }
        public double[] Candlestick { get; set; }
        public bool Label { get; set; }
        public double Value { get; set; }
        public double DayOfTheWeek { get; set; }
        public double Month { get; set; }

        public StockRiseDataFeature(double[] news,
            double[] candlestick,
            bool label,
            double value,
            double dayOfTheWeek,
            double month)
        {
            News = news;
            Candlestick = candlestick;
            Label = label;
            Value = value;
            DayOfTheWeek = dayOfTheWeek;
            Month = month;
        }
    }
}
