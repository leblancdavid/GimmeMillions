using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Keys
{
    public class AccessKey
    {

        public string Key { get; set; }
        public string Secret { get; set; }
        public string Status { get; set; }


        public AccessKey()
        {
            Status = "inactive";
        }

        public AccessKey(string key, string secret, string status)
        {
            Key = key;
            Secret = secret;
            Status = status;
        }
    }
}
