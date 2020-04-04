using GimmeMillions.Domain.Features;

namespace GimmeMillions.Domain.ML.Binary
{
    public interface IBinaryStockPredictionModel<TParams, TFeature> : IStockPredictionModel<TFeature>
        where TFeature : FeatureVector
    {
        TParams Parameters { get; set; }
        BinaryPredictionModelMetadata<TParams> Metadata { get; }
       
    }
}
