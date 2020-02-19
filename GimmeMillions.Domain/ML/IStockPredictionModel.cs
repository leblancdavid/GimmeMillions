using CSharpFunctionalExtensions;
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
        bool IsTrained { get; }
        Result<TrainingResult> Train(string symbol, DateTime startDate, DateTime endDate);
        Result<StockPrediction> Predict(DateTime date);
        Result<StockPrediction> PredictLatest();
        Result Save(string pathToModel);
        Result Load(string pathToModel, string symbol);
    }
}
