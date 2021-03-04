using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GimmeMillions.DataAccess.Clients.TDAmeritrade
{
    public class AmeritradeCredentials
    {
        public string ApiKey { get; set; }
        public string RefreshToken { get; set; }
        public string Token { get; set; }

        public AmeritradeCredentials()
        {

        }

        public AmeritradeCredentials(string apiKey, string refreshToken, string token)
        {
            ApiKey = apiKey;
            RefreshToken = refreshToken;
            Token = token;
        }

        public static bool Write(string filename, AmeritradeCredentials credentials)
        {
            try
            {
                File.WriteAllText(filename, JsonConvert.SerializeObject(credentials));
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static AmeritradeCredentials Read(string filename)
        {
            return JsonConvert.DeserializeObject<AmeritradeCredentials>(File.ReadAllText(filename));
        }
    }
}
