using GimmeMillions.Domain.Stocks;

namespace GimmeMillions.Domain.ML
{
    public class PercentDayChangeOutputMapper : ITrainingOutputMapper
    {
        private decimal _percentChangeThreshold;
        public PercentDayChangeOutputMapper(decimal percentChangeThreshold)
        {
            _percentChangeThreshold = percentChangeThreshold;
        }

        public bool GetBinaryValue(StockData stockData)
        {
            return stockData.PercentChangeFromPreviousClose > _percentChangeThreshold;
        }
      
        public float GetOutputValue(StockData stockData)
        {
            return (float)stockData.PercentChangeFromPreviousClose;
        }
    };
}
