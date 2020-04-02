using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class HistoricalFeatureDatasetService : IFeatureDatasetService<HistoricalFeatureVector>
    {
        public bool RefreshCache { get; set; }

        public IEnumerable<(HistoricalFeatureVector Input, StockData Output)> GetAllTrainingData(DateTime startDate = default, DateTime endDate = default)
        {
            throw new NotImplementedException();
        }

        public Result<(HistoricalFeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Result<HistoricalFeatureVector> GetFeatureVector(string symbol, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Result<IEnumerable<(HistoricalFeatureVector Input, StockData Output)>> GetTrainingData(string symbol, DateTime startDate = default, DateTime endDate = default)
        {
            throw new NotImplementedException();
        }
    }
}
