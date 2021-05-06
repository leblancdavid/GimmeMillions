using Accord.Neuro.ActivationFunctions;
using GimmeMillions.Domain.Stocks;

namespace GimmeMillions.Domain.ML
{
    public class BernoulliPercentChange : ITrainingOutputMapper
    {
        private double _averageGain;
        private double _averageLoss;
        private BernoulliFunction _activationFunction = new BernoulliFunction();
        public BernoulliPercentChange(double averageGain, double averageLoss)
        {
            _averageGain = averageGain;
            _averageLoss = averageLoss;
        }

        public bool GetBinaryValue(StockData stockData)
        {
            return GetOutputValue(stockData) > 0.5;
        }
      
        public float GetOutputValue(StockData stockData)
        {
            return (float)_activationFunction.Function(stockData.PercentChangeFromPreviousClose > 0.0m ?
                    (double)stockData.PercentChangeFromPreviousClose / (_averageGain) :
                    (double)stockData.PercentChangeFromPreviousClose / (_averageLoss));
        }
    };
}
