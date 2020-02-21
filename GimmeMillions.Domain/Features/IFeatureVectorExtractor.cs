using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureVectorExtractor
    {
        FeatureVector Extract(IEnumerable<(Article Article, float Weight)> articles);
    }
}
