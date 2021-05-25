using System;
using System.Collections.Generic;
using System.Text;

namespace GimmeMillions.DataAccess.Clients.TDAmeritrade
{
    public interface IUrlProvider
    {
        string GetRequestUrl(bool authenticated = false);
    }
}
