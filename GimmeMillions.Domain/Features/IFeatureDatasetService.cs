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
        Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol, 
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime));
        Result<(FeatureVector Input, StockData Output)> GetTestExample(string symbol, DateTime date);
    }
}
