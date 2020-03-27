using CSharpFunctionalExtensions;

namespace GimmeMillions.Domain.ML
{
    public interface IStockPredictionModelLoader
    {
        Result<IStockPredictionModel> LoadModel(string pathToModel, string symbol, string encoding);
    }
}
