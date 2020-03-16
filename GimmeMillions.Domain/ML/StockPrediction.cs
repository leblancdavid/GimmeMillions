namespace GimmeMillions.Domain.ML
{
    public class StockPrediction
    {
        public double Score { get; set; }
        public double AdjustedScore { get; set; }
        public bool PredictedLabel { get; set; }
        public double Probability { get; set; }

    }
}
