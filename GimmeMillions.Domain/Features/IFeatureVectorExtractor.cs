using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureVectorExtractor
    {
        string Encoding { get; }
        FeatureVector Extract(IEnumerable<(Article Article, float Weight)> articles);
    }
}
