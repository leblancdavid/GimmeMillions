namespace GimmeMillions.Domain.ML.Binary
{
    public class BinaryPredictionModelMetadata<TParams>
    {
        public ModelMetrics TrainingResults { get; set; }
        public string StockSymbol { get; set; }
        public TParams Parameters { get; set; }
        public bool IsTrained 
        { 
            get
            {
                return TrainingResults != null;
            }
        }
        public float AverageUpperProbability { get; set; }
        public float AverageLowerProbability { get; set; }
        public float AverageUpperScore { get; set; }
        public float AverageLowerScore { get; set; }
        public string FeatureEncoding { get; set; }
        public string ModelId { get; set; }
        public double PeakAverage { get; set; }
        public double PeakSelection { get; set; }
        public BinaryPredictionModelMetadata()
        {
        }
    }
}
