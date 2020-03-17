using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.ML.Binary;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public interface IStockPredictionModel
    {
        string StockSymbol { get; }
        string Encoding { get; }
        bool IsTrained { get; }

        Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction);
        StockPrediction Predict(FeatureVector Input);
        Result Save(string pathToModel);
        Result Load(string pathToModel, string symbol, string encoding);
    }
}
