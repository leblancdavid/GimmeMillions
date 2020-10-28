using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System.Collections.Generic;

namespace GimmeMillions.Domain.ML
{
    public interface IStockRangePredictor : IStockPredictionModel<FeatureVector, StockRangePrediction>
    {
    }
}
