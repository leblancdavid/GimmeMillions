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
        public TDAmeritradeApiClient(string apiKey)
        {
            ApiKey = apiKey;
        }

        public string ApiKey { get; private set; }

        public HttpResponseMessage GetPriceHistory(PriceHistoryRequest requestData)
        {
            try
            {
                Thread.Sleep(500);
                var url = requestData.GetRequestUrl();
                return Task.Run(async () => await _client.GetAsync(url)).Result;
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
