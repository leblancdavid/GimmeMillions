using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public interface IFeatureDictionaryRepository
    {
        Result AddOrUpdate(FeaturesDictionary featuresDictionary);
        Result<FeaturesDictionary> GetFeatureDictionary(string id);
        IEnumerable<string> GetFeatureDictionaryIds();
    }
}
