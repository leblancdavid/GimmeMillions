using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Stocks
{
    public class DefaultStockStatisticsCalculator : IStockStatisticsCalculator
    {
        private int _shortLength;
        private int _longLength;
        public DefaultStockStatisticsCalculator(int shortLength, int longLength)
        {
            _shortLength = shortLength;
            _longLength = longLength;
        }

        public Result<StockStatistics> Compute(IEnumerable<StockData> stockData, DateTime date)
        {
            var filteredList = stockData.Where(x => x.Date < date).OrderByDescending(y => y.Date);
            var longList = filteredList.Take(_longLength);
            var shortList = filteredList.Take(_shortLength);

            if(!longList.Any() || !shortList.Any())
            {
                return Result.Failure<StockStatistics>("Insufficient data to compute statistics");
            }

            var stats = new StockStatistics();
            stats.LongTermLength = _longLength;
            stats.ShortTermLength = _shortLength;
            stats.Date = date;
            stats.AverageShortTermVolume = shortList.Average(x => x.Volume);
            stats.AverageLongTermVolume = longList.Average(x => x.Volume);
            stats.AverageShortTermDayRange = shortList.Average(x => x.PercentDayRange);
            stats.AverageLongTermDayRange = longList.Average(x => x.PercentDayRange);
            stats.AverageShortTermTrend = shortList.Average(x => x.PercentChangeFromPreviousClose);
            stats.AverageLongTermTrend = longList.Average(x => x.PercentChangeFromPreviousClose);
            return Result.Success(stats);
        }
    }
}
