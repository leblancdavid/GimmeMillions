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

        public double AverageCount
        {
            get
            {
                return FeatureTable.Sum(x => (double)x.Value) / (double)FeatureTable.Count;
            }
        }

        public int MaxCount { get; set; }
        public string DictionaryId { get; set; }
            
        public FeaturesDictionary()
        {
            MaxCount = 0;
            DictionaryId = Guid.NewGuid().ToString();
            FeatureTable = new Dictionary<string, int>();
        }
        public void Clear()
        {
            MaxCount = 0;
            FeatureTable = new Dictionary<string, int>();
        }

        public void AddArticle(Article article, ITextProcessor textProcessor)
        {
            ProcessFeatureString(article.Abstract, textProcessor);
            ProcessFeatureString(article.Snippet, textProcessor);
            ProcessFeatureString(article.LeadParagraph, textProcessor);
        }

        private void ProcessFeatureString(string text, ITextProcessor textProcessor)
        {
            var features = textProcessor.Process(text);
            foreach(var f in features)
            {
                if(string.IsNullOrEmpty(f))
                {
                    continue;
                }

                if(FeatureTable.ContainsKey(f))
                {
                    FeatureTable[f]++;
                }
                else
                {
                    FeatureTable[f] = 1;
                }
                if(MaxCount < FeatureTable[f])
                {
                    MaxCount = FeatureTable[f];
                }
            }
        }
    }
}
