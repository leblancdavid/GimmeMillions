using Accord.Statistics.Analysis;
using Newtonsoft.Json;
using System;

namespace GimmeMillions.Domain.Features
{
    public class HistoricalFeatureVector : FeatureVector
    {
        [JsonIgnore]
        public double[] NewsData
        {
            get
            {
                return Data;
            }
            set
            {
                Data = value;
            }
        }

        public double[] CandlestickData { get; set; }

        public HistoricalFeatureVector(double[] newsData,
            double[] candlestickData,
            DateTime date,
            string encoding) : base(newsData, date, encoding)
        {
            CandlestickData = candlestickData;
        }

        public HistoricalFeatureVector()
        {
        }

        public double[] GetConcatenateFeature(PrincipalComponentAnalysis pca = null)
        {
            double[] newsData = NewsData;
            if (pca != null)
            {

            }

            return newsData;
        }
    }
}
