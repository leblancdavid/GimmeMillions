using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GimmeMillions.DataAccess.Features
{
    public class FeatureJsonCache : IFeatureCache
    {
        public readonly string _pathToCache;
        public FeatureJsonCache(string pathToCache)
        {
            _pathToCache = pathToCache;

        }
        public bool Exists(string encoding, DateTime date)
        {
            return File.Exists($"{_pathToCache}/{encoding}/{date.ToString("yyyy-MM-dd")}.json");
        }

        public Result<TFeature> GetFeature<TFeature>(string encoding, DateTime date) where TFeature : FeatureVector
        {
            try
            {
                string fileName = $"{_pathToCache}/{encoding}/{date.ToString("yyyy-MM-dd")}.json";
                if(!File.Exists(fileName))
                {
                    return Result.Failure<TFeature>($"No cache feature found with encoding {encoding} for {date.ToString("yyyy-MM-dd")}");
                }
                var json = File.ReadAllText(fileName);
                return Result.Ok(JsonConvert.DeserializeObject<TFeature>(json));
            }
            catch (Exception ex)
            {
                return Result.Failure<TFeature>($"Error occurred while retrieving a feature from the cache: {ex.Message}");
            }
        }


        public Result UpdateCache<TFeature>(TFeature featureVector) where TFeature : FeatureVector
        {
            try
            {
                string directory = $"{_pathToCache}/{featureVector.Encoding}/";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string fileName = $"{directory}/{featureVector.Date.ToString("yyyy-MM-dd")}.json";
                File.WriteAllText(fileName, JsonConvert.SerializeObject(featureVector, Formatting.Indented));
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error occurred while adding a feature to the cache: {ex.Message}");
            }

        }
    }
}
