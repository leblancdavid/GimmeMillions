using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public interface IDatasetFilter
    {
        bool Pass(StockData stockData);
    }
    public interface IFeatureDatasetService<TFeature> where TFeature : FeatureVector
    {
        bool RefreshCache { get; set; }
        IStockAccessService StockAccess { get; }

        IEnumerable<(TFeature Input, StockData Output)> GetAllTrainingData(IDatasetFilter filter = null,
            bool updateStocks = false);
        Result<IEnumerable<(TFeature Input, StockData Output)>> GetTrainingData(string symbol,
            IDatasetFilter filter =  null,
            bool updateStocks = false);
        Result<(TFeature Input, StockData Output)> GetData(string symbol, DateTime date);
        Result<TFeature> GetFeatureVector(string symbol, DateTime date);
    }
}
