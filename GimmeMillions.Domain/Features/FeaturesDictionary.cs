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

        public void AddArticle(Article article)
        {
            ProcessFeatureString(article.Abstract);
            ProcessFeatureString(article.Snippet);
            ProcessFeatureString(article.LeadParagraph);
        }

        private void ProcessFeatureString(string feature)
        {
            var onlyLetters = Regex.Replace(feature.ToLower(), @"[^a-z]+", " ");
            var words = onlyLetters.Split(' ');
            foreach(var word in words)
            {
                if(string.IsNullOrEmpty(word))
                {
                    continue;
                }

                if(_featureSet.ContainsKey(word))
                {
                    _featureSet[word]++;
                }
                else
                {
                    _featureSet[word] = 1;
                }
            }
        }
    }
}
