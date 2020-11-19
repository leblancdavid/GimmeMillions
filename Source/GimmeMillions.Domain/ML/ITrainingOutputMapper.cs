using GimmeMillions.Domain.Stocks;

namespace GimmeMillions.Domain.ML
{
    public interface ITrainingOutputMapper
    {
        float GetOutputValue(StockData stockData);
        bool GetBinaryValue(StockData stockData);
    }
}
