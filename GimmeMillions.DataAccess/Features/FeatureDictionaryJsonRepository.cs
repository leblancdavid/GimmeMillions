using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.DataAccess.Features
{
    public class FeatureDictionaryJsonRepository : IFeatureDictionaryRepository
    {
        private readonly string _pathToDictionaries;
        public FeatureDictionaryJsonRepository(string pathToDictionaries)
        {
            _pathToDictionaries = pathToDictionaries;
        }

        public Result AddOrUpdate(FeaturesDictionary featuresDictionary)
        {
            try
            {
                string fileName = $"{_pathToDictionaries}/{featuresDictionary.DictionaryId}.json";
                File.WriteAllText(fileName, JsonConvert.SerializeObject(featuresDictionary, Formatting.Indented));
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error occurred while adding or updating a feature dictionary: {ex.Message}");
            }
        }

        public Result<FeaturesDictionary> GetFeatureDictionary(string id)
        {
            string fileName = $"{_pathToDictionaries}/{id}.json";
            if (!File.Exists(fileName))
            {
                return Result.Failure<FeaturesDictionary>($"Feature dictionary {id} could not be found");
            }
            var json = File.ReadAllText(fileName);
            return Result.Ok(JsonConvert.DeserializeObject<FeaturesDictionary>(json));
        }

        public IEnumerable<string> GetFeatureDictionaryIds()
        {
            return Directory.GetFiles(_pathToDictionaries)
                .Select(x => Path.GetFileNameWithoutExtension(x));
        }
    }
}
