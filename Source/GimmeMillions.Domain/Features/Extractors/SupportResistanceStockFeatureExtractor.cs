using GimmeMillions.Domain.Features.Extractors;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class SupportResistanceStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }
        private decimal _threshold = 0.01m;

        public SupportResistanceStockFeatureExtractor(decimal threshold = 0.01m)
        {
            Encoding = "SupportResistance";
            _threshold = threshold;
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            if(!data.Any())
            {
                return new double[] { 0.0, 0.0 };
            }
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            var resistance = new List<int>();
            var support = new List<int>();

            var last = ordered.FirstOrDefault().Data.Average;
            GetPivots(ordered, out resistance, out support);

            decimal atSupportVolume = 0.0m;
            decimal atResistanceVolume = 0.0m;
            foreach(var r in resistance)
            {
                var d = Math.Abs(ordered[r].Data.High - last) / last;
                if(d < _threshold)
                {
                    atResistanceVolume += ordered[r].Data.Volume;
                }
            }

            foreach (var s in support)
            {
                var d = Math.Abs(ordered[s].Data.Low - last) / last;
                if (d < _threshold)
                {
                    atSupportVolume += ordered[s].Data.Volume;
                }
            }

            var averageVolume = ordered.Average(x => x.Data.Volume);

            return new double[] { 
                (double)(atSupportVolume / averageVolume), 
                (double)(atResistanceVolume / averageVolume)
            };
        }

        private void GetPivots(List<(StockData Data, float Weight)> data,
            out List<int> resistance,
            out List<int> support)
        {
            resistance = new List<int>();
            support = new List<int>();

            for(int i = 2; i < data.Count - 2; ++i)
            {
                if(data[i].Data.High > data[i - 1].Data.High && 
                    data[i].Data.High > data[i + 1].Data.High)
                {
                    resistance.Add(i);
                }

                if (data[i].Data.Low < data[i - 1].Data.Low &&
                    data[i].Data.Low < data[i + 1].Data.Low)
                {
                    support.Add(i);
                }
            }
        }
    }
}
