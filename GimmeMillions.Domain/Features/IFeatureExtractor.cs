using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureExtractor<TData>
    {
        string Encoding { get; }
        float[] Extract(IEnumerable<(TData Data, float Weight)> data);
    }
}
