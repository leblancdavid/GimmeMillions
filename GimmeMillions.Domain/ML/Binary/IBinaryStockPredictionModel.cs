namespace GimmeMillions.Domain.ML.Binary
{
    public interface IBinaryStockPredictionModel<TParams> : IStockPredictionModel
    {
        TParams Parameters { get; set; }
        BinaryPredictionModelMetadata<TParams> Metadata { get; }
       
    }
}
