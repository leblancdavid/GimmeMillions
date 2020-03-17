using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Binary
{
    public interface IBinaryStockPredictionModel<TParams>
    {
        TParams Parameters { get; set; }
        BinaryPredictionModelMetadata<TParams> Metadata { get; }

        Result<ModelMetrics> Train(IEnumerable<(FeatureVector Input, StockData Output)> dataset, double testFraction);
        StockPrediction Predict(FeatureVector Input);
        Result Save(string pathToModel);
        Result Load(string pathToModel, string symbol, string encoding);
    }
}
