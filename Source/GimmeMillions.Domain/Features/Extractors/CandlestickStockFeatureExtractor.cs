﻿using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class CandlestickStockFeatureExtractor : IFeatureExtractor<StockData>
    {
        private int _version = 2;
        private bool _normalize = false;
        public string Encoding { get; set; }
        public int OutputLength { get; private set; }

        public CandlestickStockFeatureExtractor(int version = 2, bool normalize = false)
        {
            _version = version;
            _normalize = normalize;
            Encoding = $"Candlestick{_normalize}-v{_version}";

        }

        public double[] Extract(IEnumerable<(StockData Data, float Weight)> stocks)
        {
            if(!stocks.Any())
            {
                return new double[0];
            }

            var ordered = stocks.OrderBy(x => x.Data.Date).ToList();
            var lastStock = ordered.Last();

            //decimal average = (lastStock.Data.Close + lastStock.Data.Open + lastStock.Data.Low + lastStock.Data.High) / 4.0m;
            var average = stocks.Average(x => x.Data.Close);
            decimal averageVolume = ordered.Average(x => x.Data.Volume);
            var feature = new double[stocks.Count() * 5];
            int index = 0;
            foreach(var stock in ordered)
            {
                feature[index * 5] = (double)((stock.Data.Open - average) / average);
                feature[index * 5 + 1] = (double)((stock.Data.Close - average) / average);
                feature[index * 5 + 2] = (double)((stock.Data.High - average) / average);
                feature[index * 5 + 3] = (double)((stock.Data.Low - average) / average);
                feature[index * 5 + 4] = (double)((stock.Data.Volume - averageVolume) / averageVolume); 

                index++;
            }

            if (_normalize)
                return Normalize(feature);

            OutputLength = feature.Length;

            return feature;
        }

        private double[] Normalize(double[] feature)
        {
            var output = new double[feature.Length];
            double stdev = Math.Sqrt(feature.Sum(x => Math.Pow(x, 2)));
            if(stdev < 0.001)
            {
                stdev = 1.0;
            }

            for(int i = 0; i < feature.Length; ++i)
            {
                output[i] = (feature[i]) / stdev;
            }

            return output;
        }

    }
}
