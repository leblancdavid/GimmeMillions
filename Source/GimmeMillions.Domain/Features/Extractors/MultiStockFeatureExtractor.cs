using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class MultiStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }

        private IEnumerable<IFeatureExtractor<StockData>> _featureExtractors;
        public int OutputLength { get; private set; }

        public MultiStockFeatureExtractor(IEnumerable<IFeatureExtractor<StockData>> featureExtractors)
        {
            Encoding = string.Join('-', featureExtractors.Select(x => x.Encoding));
            _featureExtractors = featureExtractors;
            OutputLength = _featureExtractors.Sum(x => x.OutputLength);
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date);

            var features = new double[0];
            foreach (var extractor in _featureExtractors)
            {
                features = features.Concat(extractor.Extract(ordered)).ToArray();
            }

            return features;
        }
    }
}
