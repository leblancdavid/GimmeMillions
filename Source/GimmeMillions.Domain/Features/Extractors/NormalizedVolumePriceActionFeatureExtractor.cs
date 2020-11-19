using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class NormalizedVolumePriceActionFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private int _length;
        public NormalizedVolumePriceActionFeatureExtractor(int length)
        {
            _length = length;
            Encoding = $"NVPA{_length}-v1";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).Take(_length).ToList();

            var priceMean = ordered.Sum(x => (x.Data.Close + x.Data.Open + x.Data.High + x.Data.Low) / 4.0m ) 
                / (decimal)_length;
            var priceStdev = Math.Sqrt(ordered.Sum(
                    x => Math.Pow((double)(((x.Data.Close + x.Data.Open + x.Data.High + x.Data.Low) / 4.0m) - priceMean), 2.0))
                / (double)_length);

            var volumeMean = ordered.Sum(x => x.Data.Volume) / (decimal)_length; 
            var volumeStdev = Math.Sqrt(ordered.Sum(x => Math.Pow((double)(x.Data.Volume - volumeMean), 2.0)) / (double)_length);

            var featureVector = new List<double>();
            foreach(var d in ordered)
            {
                featureVector.Add((double)(d.Data.Open - priceMean) / priceStdev);
                featureVector.Add((double)(d.Data.Close - priceMean) / priceStdev);
                featureVector.Add((double)(d.Data.High - priceMean) / priceStdev);
                featureVector.Add((double)(d.Data.Low - priceMean) / priceStdev);
                featureVector.Add((double)(d.Data.Volume - volumeMean) / volumeStdev);
            }

            return featureVector.ToArray();
        }
    }
}
