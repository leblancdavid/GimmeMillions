using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Keys;
using System;
using System.Collections.Generic;

namespace GimmeMillions.DataAccess.Keys
{
    public class NYTApiAccessKeyRepository : IAccessKeyRepository
    {
        private string _pathToKeys;
        public NYTApiAccessKeyRepository(string pathToKeys)
        {
            _pathToKeys = pathToKeys;
        }

        public Result AddKey(AccessKey key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AccessKey> GetActiveKeys()
        {
            throw new NotImplementedException();
        }

        public Result<AccessKey> GetKey(string key)
        {
            throw new NotImplementedException();
        }

        public Result UpdateKeyStatus(string key, string status)
        {
            throw new NotImplementedException();
        }
    }
}
