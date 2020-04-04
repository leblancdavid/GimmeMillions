using CSharpFunctionalExtensions;
using System;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureCache<TFeature>
            where TFeature : FeatureVector
    {
        bool Exists(string encoding, DateTime date);
        Result<TFeature> GetFeature(string encoding, DateTime date);
        Result UpdateCache(TFeature featureVector);

    }
}
