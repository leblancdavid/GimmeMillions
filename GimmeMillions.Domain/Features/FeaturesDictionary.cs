using GimmeMillions.Domain.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class FeaturesDictionary
    {
        private Dictionary<string, int> _featureSet = new Dictionary<string, int>();

        public int Size
        {
            get
            {
                return _featureSet.Count;
            }
        }

        public double AverageCount
        {
            get
            {
                return _featureSet.Sum(x => (double)x.Value) / (double)_featureSet.Count;
            }
        }
        public void Clear()
        {
            _featureSet = new Dictionary<string, int>();
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

                if(_featureSet.ContainsKey(f))
                {
                    _featureSet[f]++;
                }
                else
                {
                    _featureSet[f] = 1;
                }
            }
        }
    }
}
