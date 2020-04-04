using Newtonsoft.Json;
using System;

namespace GimmeMillions.Domain.Features
{
    public class HistoricalFeatureVector : FeatureVector
    {
        [JsonIgnore]
        public float[] NewsData
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

        public float[] CandlestickData { get; set; }

        public HistoricalFeatureVector(float[] newsData,
            float[] candlestickData,
            DateTime date,
            string encoding) : base(newsData, date, encoding)
        {
            CandlestickData = candlestickData;
        }

        public HistoricalFeatureVector()
        {
        }
    }
}
