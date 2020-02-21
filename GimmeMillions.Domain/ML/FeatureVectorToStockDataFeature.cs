﻿using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class FeatureVectorToStockDataFeature
    {
        public float[] Features { get; set; }
        public float Label { get; set; }

        public FeatureVectorToStockDataFeature(float[] input, float label)
        {
            Features = input;
            Label = label;
        }
    }
}
