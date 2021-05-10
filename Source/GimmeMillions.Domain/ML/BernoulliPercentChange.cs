﻿using Accord.Neuro.ActivationFunctions;
using GimmeMillions.Domain.Stocks;

namespace GimmeMillions.Domain.ML
{
    public class BernoulliPercentChange : ITrainingOutputMapper
    {
        private double _averageGain;
        private double _medianGain;
        private BernoulliFunction _activationFunction = new BernoulliFunction();
        public BernoulliPercentChange(double averageGain, double medianGain)
        {
            _averageGain = averageGain;
            _medianGain = medianGain;
        }

        public bool GetBinaryValue(StockData stockData)
        {
            return GetOutputValue(stockData) > 0.5;
        }
      
        public float GetOutputValue(StockData stockData)
        {
            return (float)_activationFunction.Function(((double)stockData.PercentChangeFromPreviousClose - _medianGain) / _averageGain);
        }
    };
}
