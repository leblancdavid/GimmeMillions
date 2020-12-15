﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GimmeMillions.DataAccess.Clients
{
    public class TDAmeritradeApiClient
    {
        private readonly string _accessFile;
        private string _clientId;
        private string _redirectUri;
        private string _accessToken;
        private string _refreshToken;
        private readonly HttpClient _client = new HttpClient();
        public TDAmeritradeApiClient(string accessFile)
        {
            _accessFile = accessFile;
            TryReadAccessFile(accessFile);
        }

        private bool TryReadAccessFile(string accessFile)
        {
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(accessFile))
                {
                    _clientId = file.ReadLine().Split(' ')[1];
                    _redirectUri = file.ReadLine().Split(' ')[1];
                    _refreshToken = file.ReadLine().Split(' ')[1];
                    _accessToken = file.ReadLine().Split(' ')[1];

                    return true;
                }
            }
            catch (Exception)
            {
                throw new Exception($"Invalid TD Ameritrade access file specified '{accessFile}'");
            }
        }

        private bool TryUpdateAccessFile(string accessFile)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(accessFile))
                {
                    file.WriteLine($"client_id {_clientId}");
                    file.WriteLine($"redirect_uri {_redirectUri}");
                    file.WriteLine($"refresh_token {_refreshToken}");
                    file.WriteLine($"access_token {_accessToken}");

                    return true;
                }
            }
            catch (Exception)
            {
                throw new Exception($"Unable to write to TD Ameritrade access file specified '{accessFile}'");
            }
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

        public bool RefreshAuthentication()
        {
            try
            {
                var body = new AuthenticationPostBody()
                {
                    grant_type = "refresh_token",
                    refresh_token = _refreshToken,
                    access_type = "offline",
                    client_id = _clientId,
                    redirect_uri = _redirectUri
                };

                var url = "https://api.tdameritrade.com/v1/oauth2/token";
                var response = Task.Run(async () => await _client.PostAsync(url, body.ToFormData())).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var responseJson = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                _accessToken = (string)responseJson["access_token"];
                _refreshToken = (string)responseJson["refresh_token"];

                if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_refreshToken) || !TryUpdateAccessFile(_accessFile))
                {
                    return false;
                }
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
