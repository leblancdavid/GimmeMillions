using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    
    public interface IFeatureDatasetService<TFeature> where TFeature : FeatureVector
    {
        bool RefreshCache { get; set; }
        IStockAccessService StockAccess { get; }
        StockDataPeriod Period { get; }

        IEnumerable<(TFeature Input, StockData Output)> GetAllTrainingData(IStockFilter filter = null,
            bool updateStocks = false, int historyLimit = 0);
        IEnumerable<(TFeature Input, StockData Output)> GetTrainingData(string symbol,
            IStockFilter filter = null,
            bool updateStocks = false,
            int historyLimit = 0);
        Result<(TFeature Input, StockData Output)> GetData(string symbol, DateTime date, int historyLimit = 0);
        Result<TFeature> GetFeatureVector(string symbol, DateTime date, int historyLimit = 0);
        Result<TFeature> GetFeatureVector(string symbol, out StockData last, int historyLimit = 0);
        IEnumerable<TFeature> GetFeatures(string symbol);
    }
}
