using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureCache
    {
        bool Exists(string encoding, DateTime date);
        Result<FeatureVector> GetFeature(string encoding, DateTime date);
        Result UpdateCache(FeatureVector featureVector);

    }
}
