﻿using System.Collections.Generic;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureExtractor<TData>
    {
        string Encoding { get; }
        int OutputLength { get; }
        double[] Extract(IEnumerable<(TData Data, float Weight)> data);
    }
}
