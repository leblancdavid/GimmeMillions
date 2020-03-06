namespace GimmeMillions.Domain.ML
{
    public class StockPrediction
    {
        public float Score { get; set; }
        public bool PredictedLabel { get; set; }
        public float Probability { get; set; }
    }
}
