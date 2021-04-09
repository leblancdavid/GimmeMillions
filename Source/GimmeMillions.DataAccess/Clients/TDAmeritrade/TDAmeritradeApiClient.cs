using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
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
        private AmeritradeCredentials _credentials;
        private bool _useAuthentication = false;
        public TDAmeritradeApiClient(AmeritradeCredentials credentials, bool useAuthentication = false)
        {
            _credentials = credentials;
            _useAuthentication = useAuthentication;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            if(_useAuthentication)
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.Token);
        }

        public TDAmeritradeApiClient(string apiKey, bool useAuthentication = false)
        {
            _credentials = AmeritradeCredentials.Read($"{apiKey}.json");
            _useAuthentication = useAuthentication;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            if (_useAuthentication)
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.Token);
        }

        public string ApiKey 
        { 
            get
            {
                return _credentials.ApiKey;
            }
        }

        public HttpResponseMessage GetPriceHistory(IUrlProvider request)
        {
            lock(_throttleLock)
            {
                try
                {
                    Thread.Sleep(500);
                    var url = request.GetRequestUrl(_useAuthentication && !string.IsNullOrEmpty(_credentials.Token));
                    var result = Task.Run(async () => await _client.GetAsync(url)).Result;
                    if(result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var authenticationResult = RefreshAuthentication();
                        if(authenticationResult.IsSuccessStatusCode)
                        {
                            //retry the request
                            return GetPriceHistory(request);
                        }
                        return authenticationResult;
                    }
                    if(!result.IsSuccessStatusCode)
                    {
                        return result;
                    }
                    return result;
                }
                catch (Exception)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            
        }

        public HttpResponseMessage RefreshAuthentication()
        {
            try
            {
                var body = new AuthenticationPostBody()
                {
                    grant_type = "refresh_token",
                    refresh_token = _credentials.RefreshToken,
                    access_type = "offline",
                    client_id = ApiKey,
                    redirect_uri = "https://127.0.0.1"
                };

                var client = new RestClient("https://api.tdameritrade.com/v1/oauth2/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("postman-token", "f381d366-cf3f-e33a-b7ee-70d4ff6fbb68");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("body", body.ToBodyString(), "text/plain", ParameterType.RequestBody);
                var response = client.Execute(request);

                //var url = "https://api.tdameritrade.com/v1/oauth2/token";
                //var request = new HttpRequestMessage(HttpMethod.Post, url);
                //request.Headers.Add("content-type", "application/x-www-form-urlencoded");
                //request.Content = body.ToEncodedString();
                //var response = Task.Run(async () => await _client.SendAsync(request)).Result;
                //if (!response.IsSuccessStatusCode)
                //{
                //    return response;
                //}
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new HttpResponseMessage(response.StatusCode);
                }

                //var responseJson = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                var responseJson = JsonConvert.DeserializeObject<JObject>(response.Content);

                _credentials.Token = (string)responseJson["access_token"];
                _credentials.RefreshToken = (string)responseJson["refresh_token"];

                if (string.IsNullOrEmpty(_credentials.Token) || string.IsNullOrEmpty(_credentials.RefreshToken))
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.Token);

                AmeritradeCredentials.Write($"{_credentials.ApiKey}.json",  _credentials);

                return new HttpResponseMessage(response.StatusCode);

            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }

    public class AuthenticationPostBody
    {

        public string grant_type = "";
        public string refresh_token = "";
        public string access_type = "";
        public string code = "";
        public string client_id = "";
        public string redirect_uri = "";

        public StringContent ToEncodedString()
        {
            string content = $"grant_type={grant_type}&refresh_token={HttpUtility.UrlEncode(refresh_token)}&access_type={access_type}&code={HttpUtility.UrlEncode(code)}&client_id={client_id}&redirect_uri={HttpUtility.UrlEncode(redirect_uri)}";
            return new StringContent(content, Encoding.UTF8, "text/plain");
        }
        public string ToBodyString()
        {
            string content = $"grant_type={grant_type}&refresh_token={HttpUtility.UrlEncode(refresh_token)}&access_type={access_type}&code={HttpUtility.UrlEncode(code)}&client_id={client_id}&redirect_uri={HttpUtility.UrlEncode(redirect_uri)}";
            return content;
        }
    }
}
