using GimmeMillions.Domain.Articles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.DataAccess.Articles
{
    public class NYTArticleResponse
    {
        [JsonProperty(PropertyName = "copyright")]
        public string Copyright { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "response")]
        public NYTArticleResponseData Response { get; set; }

    }

    public class NYTArticleResponseData
    {
        [JsonProperty(PropertyName = "docs")]
        public List<Article> Docs { get; set; }
        [JsonProperty(PropertyName = "meta")]
        public NYTArticleResponseMetaData Meta { get; set; }
    }

    public class NYTArticleResponseMetaData
    {
        [JsonProperty(PropertyName = "hits")]
        public int Hits { get; set; }
        [JsonProperty(PropertyName = "offset")]
        public int Offset { get; set; }
        [JsonProperty(PropertyName = "time")]
        public int Time { get; set; }
    }
}
