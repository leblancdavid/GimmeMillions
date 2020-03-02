﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GimmeMillions.Domain.Articles;

namespace GimmeMillions.Domain.Features
{
    public class BagOfWordsFeatureVectorExtractor : IFeatureVectorExtractor
    {
        private FeaturesDictionary _featuresDictionary;
        private ITextProcessor _textProcessor;
        public string Encoding { get; set; }

        public BagOfWordsFeatureVectorExtractor(FeaturesDictionary featuresDictionary,
            ITextProcessor textProcessor)
        {
            _featuresDictionary = featuresDictionary;
            _textProcessor = textProcessor;
            Encoding = $"BoW-{_featuresDictionary.DictionaryId}";
        }


        public FeatureVector Extract(IEnumerable<(Article Article, float Weight)> articles)
        {
            if(!articles.Any())
            {
                return new FeatureVector(0, Encoding);
            }
            
            var vector = new FeatureVector(_featuresDictionary.Size, articles.Max(x => x.Article.Date), Encoding);

            Parallel.ForEach(articles, (article) =>
            {
                ProcessAndUpdateVector(article, vector);
            });
            
            return vector;

        }

        private void ProcessAndUpdateVector((Article Article, float Weight) article, FeatureVector vector)
        {
            var features = _textProcessor.Process(article.Article.Abstract)
                .Concat(_textProcessor.Process(article.Article.Snippet))
                .Concat(_textProcessor.Process(article.Article.LeadParagraph));
            if(article.Article.Headline != null)
            {
                features = features.Concat(_textProcessor.Process(article.Article.Headline.PrintHeadline));
            }

            foreach(var f in features)
            {
                int index = _featuresDictionary[f];
                if(index >= 0 && index < vector.Length)
                {
                    vector[index] += article.Weight;
                }
            }
        }
    }
}
