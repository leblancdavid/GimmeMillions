using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class RandomFeatureDatasetService : IFeatureDatasetService
    {
        private Random _random;
        private int _featureSize;
        public RandomFeatureDatasetService(int seed, int featureSize)
        {
            _random = new Random(seed);
            _featureSize = featureSize;
        }

        public Result<(FeatureVector Input, StockData Output)> GetData(string symbol, DateTime date)
        {
            return Result.Ok((GetNextRandomFeatureVector(), GetNextRandomStockData(symbol, date)));
        }

        public Result<FeatureVector> GetData(DateTime date)
        {
            return Result.Ok(GetNextRandomFeatureVector());
        }

        public Result<IEnumerable<(FeatureVector Input, StockData Output)>> GetTrainingData(string symbol, DateTime startDate = default, DateTime endDate = default)
        {
            var dataset = new List<(FeatureVector Input, StockData Output)>();
            var currentDate = startDate;
            while(currentDate < endDate)
            {
                dataset.Add((GetNextRandomFeatureVector(), GetNextRandomStockData(symbol, currentDate)));
                currentDate = currentDate.AddDays(1.0);
            }

            return Result.Ok<IEnumerable<(FeatureVector Input, StockData Output)>>(dataset);
        }

        private FeatureVector GetNextRandomFeatureVector()
        {
            var v = new FeatureVector(_featureSize);
            for (int i = 0; i < _featureSize; ++i)
            {
                v[i] = (float)_random.NextDouble();
            }

            return v;
        }
        
        private StockData GetNextRandomStockData(string symbol, DateTime date)
        {
            var openingPrice = (decimal)(_random.NextDouble() * 5.0);
            //Should only vary by about 5%
            var closingPrice = openingPrice + openingPrice*(decimal)((_random.NextDouble() - 0.5)/ 10.0);
            return new StockData(symbol, date,
                openingPrice,
                (decimal)42.0,
                (decimal)0.0,
                closingPrice,
                (decimal)(_random.NextDouble() * 5.0));
        }
    }
}
