using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureDatasetService
    {
        bool RefreshCache { get; set; }

        IEnumerable<(FeatureVector Input, StockData Output)> GetAllTrainingData(
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime));
        Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol, 
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime));
        Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date);
        Result<FeatureVector> GetFeatureVector(string symbol, DateTime date);
    }
}
