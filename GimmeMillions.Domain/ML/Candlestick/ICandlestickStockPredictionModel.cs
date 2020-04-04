using GimmeMillions.Domain.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Candlestick
{
    interface ICandlestickStockPredictionModel<TParams, TFeature> : IStockPredictionModel<TFeature>
        where TFeature : FeatureVector
    {
        TParams Parameters { get; set; }
        CandlestickPredictionModelMetadata<TParams> Metadata { get; }
    }
}
