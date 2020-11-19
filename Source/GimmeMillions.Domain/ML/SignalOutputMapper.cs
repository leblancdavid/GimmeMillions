using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class SignalOutputMapper : ITrainingOutputMapper
    {
        public bool GetBinaryValue(StockData stockData)
        {
            return stockData.Signal > 0.5m;
        }

        public float GetOutputValue(StockData stockData)
        {
            return (float)stockData.Signal;
        }
    };
}
