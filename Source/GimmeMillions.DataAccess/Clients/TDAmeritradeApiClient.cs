using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.DataAccess.Clients
{
    public class TDAmeritradeApiClient
    {
        private readonly string _clientId;
        private readonly string _secretCode;
        private readonly string _redirectUri;
        private string _accessToken;
        private string _refreshToken;
        private readonly HttpClient _client = new HttpClient();
        public TDAmeritradeApiClient(string clientId, string secretCode, string redirectUri)
        {
            _clientId = clientId;
            _secretCode = secretCode;
            _redirectUri = redirectUri;
        }

        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrEmpty(_accessToken);
            }
        }

        private struct AuthenticationPostBody
        {
            public string grant_type;
            public string refresh_token;
            public string access_type;
            public string code;
            public string client_id;
            public string redirect_uri;
        }

        public bool Authenticate()
        {
            try
            {
                var body = new AuthenticationPostBody()
                {
                    grant_type = "authorization_code",
                    access_type = "offline",
                    code = _secretCode,
                    client_id = _clientId,
                    redirect_uri = _redirectUri
                };

                var url = "https://api.tdameritrade.com/v1/oauth2/token";
                var response = Task.Run(async () => await _client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"))).Result;
                if(!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var responseJson = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                _accessToken = (string)responseJson["access_token"];
                _refreshToken = (string)responseJson["refresh_token"];
                if(string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_refreshToken))
                {
                    return false;
                }
                return true;

            }
            catch(Exception ex)
            {
                return false;
            }

        }

        public bool RefreshAuthentication()
        {
            try
            {
                var body = new AuthenticationPostBody()
                {
                    grant_type = "refresh_token",
                    refresh_token = _refreshToken,
                    client_id = _clientId,
                    redirect_uri = _redirectUri
                };

                var url = "https://api.tdameritrade.com/v1/oauth2/token";
                var response = Task.Run(async () => await _client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"))).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var responseJson = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                _accessToken = (string)responseJson["access_token"];
                _refreshToken = (string)responseJson["refresh_token"];
                if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_refreshToken))
                {
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
