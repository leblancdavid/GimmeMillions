using GimmeMillions.Domain.ML.Binary;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Candlestick
{
    public class CandlestickPredictionModelMetadata<TParams>
    {
        public ModelMetrics TrainingResults { get; set; }
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

        public CandlestickPredictionModelMetadata()
        {
        }
    }
}
