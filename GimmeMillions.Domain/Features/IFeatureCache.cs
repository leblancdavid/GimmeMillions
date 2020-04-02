using CSharpFunctionalExtensions;
using System;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureCache
    {
        bool Exists(string encoding, DateTime date);
        Result<TFeature> GetFeature<TFeature>(string encoding, DateTime date) 
            where TFeature : FeatureVector;
        Result UpdateCache<TFeature>(TFeature featureVector)
            where TFeature : FeatureVector;

    }
}
