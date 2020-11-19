using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using System.Collections.Generic;

namespace GimmeMillions.Domain.ML
{
    public interface IStockPredictionModel<TFeature, TPrediction> where TFeature: FeatureVector
    {
        bool IsTrained { get; }

        Result<ModelMetrics> Train(IEnumerable<(TFeature Input, StockData Output)> dataset, 
            double testFraction, 
            ITrainingOutputMapper trainingOutputMapper);
        TPrediction Predict(TFeature Input);
        Result Save(string pathToModel);
        Result Load(string pathToModel);
    }
}
