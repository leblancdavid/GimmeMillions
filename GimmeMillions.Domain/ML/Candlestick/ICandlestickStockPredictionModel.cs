using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Candlestick
{
    interface ICandlestickStockPredictionModel<TParams> : IStockPredictionModel
    {
        TParams Parameters { get; set; }
        CandlestickPredictionModelMetadata<TParams> Metadata { get; }
    }
}
