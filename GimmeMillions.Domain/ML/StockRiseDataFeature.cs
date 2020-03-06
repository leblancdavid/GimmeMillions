using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class StockRiseDataFeature
    {
        public float[] Features { get; set; }
        public bool Label { get; set; }
        public float Value { get; set; }
        public float DayOfTheWeek { get; set; }
        public float Month { get; set; }

        public StockRiseDataFeature(float[] input, 
            bool label, 
            float value, 
            float dayOfTheWeek,
            float month)
        {
            Features = input;
            Label = label;
            Value = value;
            DayOfTheWeek = dayOfTheWeek;
            Month = month;
        }
    }
}
