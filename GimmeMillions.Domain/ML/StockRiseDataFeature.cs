﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class StockRiseDataFeature
    {
        public float[] News { get; set; }
        public float[] Candlestick { get; set; }
        public bool Label { get; set; }
        public float Value { get; set; }
        public float DayOfTheWeek { get; set; }
        public float Month { get; set; }

        public StockRiseDataFeature(float[] news,
            float[] candlestick,
            bool label, 
            float value, 
            float dayOfTheWeek,
            float month)
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
