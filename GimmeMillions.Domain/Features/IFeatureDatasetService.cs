﻿using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureDatasetService<TFeature> where TFeature : FeatureVector
    {
        bool RefreshCache { get; set; }

        IEnumerable<(TFeature Input, StockData Output)> GetAllTrainingData(
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime));
        Result<IEnumerable<(TFeature Input, StockData Output)>> GetTrainingData(string symbol, 
            DateTime startDate = default(DateTime), DateTime endDate = default(DateTime));
        Result<(TFeature Input, StockData Output)> GetData(string symbol, DateTime date);
        Result<TFeature> GetFeatureVector(string symbol, DateTime date);
    }
}
