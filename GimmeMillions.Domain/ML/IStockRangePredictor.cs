using GimmeMillions.Domain.Features;

namespace GimmeMillions.Domain.ML
{
    public interface IStockRangePredictor : IStockPredictionModel<FeatureVector, StockRangePrediction>
    {
    }
}
