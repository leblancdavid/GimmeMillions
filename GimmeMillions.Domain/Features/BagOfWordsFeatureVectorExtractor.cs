using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GimmeMillions.Domain.Articles;

namespace GimmeMillions.Domain.Features
{
    public class BagOfWordsFeatureVectorExtractor : IFeatureExtractor<Article>
    {
        private FeaturesDictionary _featuresDictionary;
        private ITextProcessor _textProcessor;
        private int _version = 2;
        public string Encoding { get; set; }

        public BagOfWordsFeatureVectorExtractor(FeaturesDictionary featuresDictionary,
            ITextProcessor textProcessor,
            int version = 2)
        {
            _featuresDictionary = featuresDictionary;
            _textProcessor = textProcessor;
            _version = version;
            Encoding = $"BoW-v{_version}-{_featuresDictionary.DictionaryId}";
        }


        public float[] Extract(IEnumerable<(Article Data, float Weight)> data)
        {
            if(!data.Any())
            {
                return new float[0];
            }

            var extractedVector = new float[_featuresDictionary.Size];

            Parallel.ForEach(data, (article) =>
            {
                ProcessAndUpdateVector(article, extractedVector);
            });

            for (int i = 0; i < extractedVector.Length; ++i)
            {
                extractedVector[i] /= data.Count();
            }

            return extractedVector;

        }

        private void ProcessAndUpdateVector((Article Article, float Weight) article, float[] vector)
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
