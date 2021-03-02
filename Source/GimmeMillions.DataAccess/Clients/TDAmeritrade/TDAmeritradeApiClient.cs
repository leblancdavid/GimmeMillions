using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GimmeMillions.DataAccess.Clients.TDAmeritrade
{
    public class TDAmeritradeApiClient
    {
        private readonly HttpClient _client = new HttpClient();
        private object _throttleLock = new object();
        private string _token;
        public TDAmeritradeApiClient(string apiKey, string token = null)
        { 
            ApiKey = apiKey;
            _token = token;
            if (!string.IsNullOrEmpty(_token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public string ApiKey { get; private set; }

        public HttpResponseMessage GetPriceHistory(IUrlProvider request)
        {
            lock(_throttleLock)
            {
                try
                {
                    Thread.Sleep(500);
                    var url = request.GetRequestUrl(!string.IsNullOrEmpty(_token));
                    return Task.Run(async () => await _client.GetAsync(url)).Result;
                }
                catch (Exception)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            
        }
    }
}
