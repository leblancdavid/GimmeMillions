using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Keys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace GimmeMillions.DataAccess.Keys
{
    public class NYTApiAccessKeyRepository : IAccessKeyRepository
    {
        private string _pathToKeys;
        public NYTApiAccessKeyRepository(string pathToKeys)
        {
            _pathToKeys = pathToKeys;
        }

        public Result AddOrUpdateKey(AccessKey key)
        {
            try
            {
                string fileName = $"{_pathToKeys}/{key.Key}.json";
                File.WriteAllText(fileName, JsonConvert.SerializeObject(key, Formatting.Indented));
                return Result.Ok();
            }
            catch(Exception ex)
            {
                return Result.Failure($"Error occurred while adding or updating an access key: {ex.Message}");
            }
        }

        public IEnumerable<AccessKey> GetActiveKeys()
        {
            var accessKeys = new List<AccessKey>();
            var keyFiles = Directory.GetFiles(_pathToKeys);

            foreach(var keyFile in keyFiles)
            {
                var jsonKey = File.ReadAllText(keyFile);
                var key = JsonConvert.DeserializeObject<AccessKey>(jsonKey);
                if(key.Status.ToLower() == "active")
                {
                    accessKeys.Add(key);
                }
            }

            return accessKeys;
        }

        public Result<AccessKey> GetKey(string key)
        {
            string fileName = $"{_pathToKeys}/{key}.json";
            if(!File.Exists(fileName))
            {
                return Result.Failure<AccessKey>($"Key {key} could not be found");
            }
            var jsonKey = File.ReadAllText(fileName);
            return Result.Ok(JsonConvert.DeserializeObject<AccessKey>(jsonKey));
        }

        public Result UpdateKeyStatus(string key, string status)
        {
            return AddOrUpdateKey(new AccessKey(key, status));
        }
    }
}
