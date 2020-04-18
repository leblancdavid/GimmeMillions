namespace GimmeMillions.Domain.ML.Candlestick
{
    public class StockCandlestickDataFeature
    {
        public float[] Features { get; set; }
        public bool Label { get; set; }
        public float Value { get; set; }
        public float DayOfTheWeek { get; set; }
        public float Month { get; set; }

        public StockCandlestickDataFeature(float[] features,
            bool label,
            float value,
            float dayOfTheWeek,
            float month)
        {
            Features = features;
            Label = label;
            Value = value;
            DayOfTheWeek = dayOfTheWeek;
            Month = month;
        }
    }
}
