﻿using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Binary
{
    public class BinaryPredictionModelMetadata<TParams>
    {
        public ModelMetrics TrainingResults { get; set; }
        public string StockSymbol { get; set; }
        public TParams Parameters { get; set; }
        public bool IsTrained 
        { 
            get
            {
                return TrainingResults != null;
            }
        }
        public float AverageUpperProbability { get; set; }
        public float AverageLowerProbability { get; set; }
        public float AverageUpperScore { get; set; }
        public float AverageLowerScore { get; set; }
        public string FeatureEncoding { get; set; }
        public string ModelId { get; set; }

        public BinaryPredictionModelMetadata()
        {
        }
    }
}
