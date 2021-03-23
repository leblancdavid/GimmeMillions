namespace GimmeMillions.Domain.ML
{
    public class StockRangePrediction
    {
        public double PredictedLow { get; set; }
        public double PredictedHigh { get; set; }
        public double Sentiment { get; set; }
        public double Confidence { get; set; }
    }
}
