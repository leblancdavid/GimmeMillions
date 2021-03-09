using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Features.Extractors
{
    public class DFTStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        public string Encoding { get; private set; }

        public DFTStockFeatureExtractor()
        {
            Encoding = "DFTFeatures";
        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> data)
        {
            var ordered = data.OrderByDescending(x => x.Data.Date).ToList();
            double minPrice = data.Min(x => (double)x.Data.Low);
            double maxPrice = data.Max(x => (double)x.Data.High);
            double minVolume = data.Min(x => (double)x.Data.Volume);
            double maxVolume = data.Max(x => (double)x.Data.Volume);

            var dft = DFT(ordered.Select(x => ((double)x.Data.Close - minPrice) / (maxPrice - minPrice)).ToArray())
                .Concat(DFT(ordered.Select(x => ((double)x.Data.Open - minPrice) / (maxPrice - minPrice)).ToArray()))
                .Concat(DFT(ordered.Select(x => ((double)x.Data.High - minPrice) / (maxPrice - minPrice)).ToArray()))
                .Concat(DFT(ordered.Select(x => ((double)x.Data.Low - minPrice) / (maxPrice - minPrice)).ToArray()))
                .Concat(DFT(ordered.Select(x => ((double)x.Data.Volume - minVolume) / (maxPrice - maxVolume)).ToArray())).ToArray();

            return dft;
        }

        private double[] DFT(double[] input, int partials = 0)
        {
            int len = input.Length;
            double[] cosDFT = new double[len / 2 + 1];
            double[] sinDFT = new double[len / 2 + 1];

            if (partials == 0)
                partials = len / 2;

            for (int n = 0; n <= partials; n++)
            {
                double cos = 0.0;
                double sin = 0.0;

                for (int i = 0; i < len; i++)
                {
                    cos += input[i] * Math.Cos(2 * Math.PI * n / len * i);
                    sin += input[i] * Math.Sin(2 * Math.PI * n / len * i);
                }

                cosDFT[n] = cos;
                sinDFT[n] = sin;
            }

            return cosDFT.Concat(sinDFT).ToArray();
        }
    }
}
