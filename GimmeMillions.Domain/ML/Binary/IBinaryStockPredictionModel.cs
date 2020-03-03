using CSharpFunctionalExtensions;
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
        string StockSymbol { get; }
        bool IsTrained { get; }
        TParams Parameters { get; set; }

        Result<BinaryClassificationMetrics> Train(DateTime startDate, DateTime endDate, double testFraction);
        Result<StockPrediction> Predict(DateTime date);
        Result<StockPrediction> PredictLatest();
        Result Save(string pathToModel);
        Result Load(string pathToModel);
    }
}
