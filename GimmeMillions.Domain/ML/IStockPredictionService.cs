using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public interface IStockPredictionService
    {
        IStockPredictionModel Model { get; set; }
        Result<StockPrediction> GetLatestPrediction();

    }
}
