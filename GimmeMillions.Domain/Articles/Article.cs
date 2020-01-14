using System.Collections.Generic;

namespace GimmeMillions.Domain.Articles
{
    public class Article
    {
        public string Abstract { get; set; }
        public string WebUrl { get; set; }
        public string Snippet { get; set; }
        public string LeadParagraph { get; set; }
        public string PrintSection { get; set; }
        public string PrintPage { get; set; }
        public string Source { get; set; }
        public string PubDate { get; set; }
        public string DocumentType { get; set; }
        public string NewsDesk { get; set; }
        public string SectionName { get; set; }
        public string TypeOfMaterial { get; set; }
        public string Uri { get; set; }
        public int WordCount { get; set; }
        public Headline Headline { get; set; }
        public List<Keyword> Keywords { get; set; }
        public List<Multimedia> Multimedia { get; set; }
        public Byline Byline { get; set; }
    }
}
