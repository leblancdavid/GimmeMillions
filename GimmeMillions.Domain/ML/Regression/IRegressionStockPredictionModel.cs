using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System.Collections.Generic;

namespace GimmeMillions.Domain.ML.Regression
{
    public interface IRegressionStockPredictionModel
    {
        string StockSymbol { get; }
        string Encoding { get; }
        bool IsTrained { get; }

        Result<MLRegressionMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction);
        StockRegressionPrediction Predict(FeatureVector Input);
        Result Save(string pathToModel);
        Result Load(string pathToModel, string symbol, string encoding);
    }
}
