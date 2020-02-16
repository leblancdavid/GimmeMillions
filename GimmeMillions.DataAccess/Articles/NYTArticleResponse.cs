using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.DataAccess.Articles
{
    public class NYTArticleResponse
    {
        public string Copyright { get; set; }
        public string Status { get; set; }
        public NYTArticleResponseData Response { get; set; }

    }

    public class NYTArticleResponseData
    {
        public List<Article> Docs { get; set; }
        public NYTArticleResponseMetaData Meta { get; set; }
    }

    public class NYTArticleResponseMetaData
    {
        public int Hits { get; set; }
        public int Offset { get; set; }
        public int Time { get; set; }
    }
}
