using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public interface IStockFeatureExtractor
    {
        string Encoding { get; }
        FeatureVector Extract(IEnumerable<StockData> stocks);
    }
}
