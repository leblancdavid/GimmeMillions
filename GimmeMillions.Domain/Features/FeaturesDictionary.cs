using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class FeaturesDictionary
    {
        public Dictionary<string, int> FeatureTable { get; set; }

        public int Size
        {
            get
            {
                return FeatureTable.Count;
            }
        }

        public string DictionaryId { get; set; }

        public int this[string i]
        {
            get
            {
                if(!FeatureTable.ContainsKey(i))
                {
                    return -1;
                }

                return FeatureTable[i];
            }
            private set { FeatureTable[i] = value; }
        }

        public FeaturesDictionary()
        {
            DictionaryId = Guid.NewGuid().ToString();
            FeatureTable = new Dictionary<string, int>();
        }
        public void Clear()
        {
            FeatureTable = new Dictionary<string, int>();
        }

        public void AddArticle(Article article, ITextProcessor textProcessor)
        {
            ProcessFeatureString(article.Abstract, textProcessor);
            ProcessFeatureString(article.Snippet, textProcessor);
            ProcessFeatureString(article.LeadParagraph, textProcessor);
            if(article.Headline != null)
            {
                ProcessFeatureString(article.Headline.PrintHeadline, textProcessor);
            }
        }

        private void ProcessFeatureString(string text, ITextProcessor textProcessor)
        {
            int index = FeatureTable.Count;
            var features = textProcessor.Process(text);
            foreach (var f in features)
            {
                if (string.IsNullOrEmpty(f))
                {
                    continue;
                }

                if (!FeatureTable.ContainsKey(f))
                {
                    FeatureTable[f] = index;
                    index++;
                }
            }
        }
    }
}
