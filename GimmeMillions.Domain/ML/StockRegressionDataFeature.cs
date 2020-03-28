namespace GimmeMillions.Domain.ML
{
    public class StockRegressionDataFeature
    {
        public float[] Features { get; set; }
        public float Label { get; set; }
        public float DayOfTheWeek { get; set; }
        public float Month { get; set; }

        public StockRegressionDataFeature(float[] input,
            float label, 
            float dayOfTheWeek,
            float month)
        {
            Features = input;
            Label = label;
            DayOfTheWeek = dayOfTheWeek;
            Month = month;
        }
    }
}
