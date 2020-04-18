namespace GimmeMillions.Domain.ML.Candlestick
{
    public class StockCandlestickRegressionFeature
    {
        public float[] Features { get; set; }
        public float Label { get; set; }
        public float DayOfTheWeek { get; set; }
        public float Month { get; set; }

        public StockCandlestickRegressionFeature(float[] features,
            float label,
            float dayOfTheWeek,
            float month)
        {
            Features = features;
            Label = label;
            DayOfTheWeek = dayOfTheWeek;
            Month = month;
        }
    }
}
