using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System.Collections.Generic;

namespace GimmeMillions.Domain.ML
{
    public interface IStockPredictionModel<TFeature> where TFeature: FeatureVector
    {
        string Encoding { get; }
        bool IsTrained { get; }

        Result<ModelMetrics> Train(IEnumerable<(TFeature Input, StockData Output)> dataset, double testFraction);
        StockPrediction Predict(TFeature Input);
        Result Save(string pathToModel);
        Result Load(string pathToModel, string symbol, string encoding);
    }
}
