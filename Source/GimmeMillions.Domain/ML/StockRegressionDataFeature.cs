namespace GimmeMillions.Domain.ML
{
    public class StockRegressionDataFeature
    {
        public double[] Features { get; set; }
        public double Label { get; set; }
        public double DayOfTheWeek { get; set; }
        public double Month { get; set; }

        public StockRegressionDataFeature(double[] input,
            double label,
            double dayOfTheWeek,
            double month)
        {
            Features = input;
            Label = label;
            DayOfTheWeek = dayOfTheWeek;
            Month = month;
        }
    }
}
