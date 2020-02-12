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
        Result AddKey(AccessKey key);
        IEnumerable<AccessKey> GetActiveKeys();
        Result<AccessKey> GetKey(string key);
        Result UpdateKeyStatus(string key, string status);
    }
}
