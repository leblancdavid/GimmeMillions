using GimmeMillions.Domain.Features.Services;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class FuturesFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }

        public int OutputLength { get; private set; }

        private FuturesFeatureDatasetCache _futuresCache;

        public FuturesFeatureExtractor(FuturesFeatureDatasetCache futuresCache)
        {
            _futuresCache = futuresCache;
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var lastDate = data.Max(x => x.Data.Date);
            var features = _futuresCache.GetFuturesFor(lastDate.Date);

            if(features.IsFailure)
            {
                return new double[0];
            }

            return features.Value.Data;
        }
    }
}
