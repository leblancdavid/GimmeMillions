using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Keys
{
    public interface IAccessKeyRepository
    {
        Result AddOrUpdateKey(AccessKey key);
        IEnumerable<AccessKey> GetActiveKeys();
        IEnumerable<AccessKey> GetKeys();
        Result<AccessKey> GetKey(string key);
    }
}
