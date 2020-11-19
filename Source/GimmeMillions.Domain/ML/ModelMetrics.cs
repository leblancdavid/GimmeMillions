using Microsoft.ML.Data;

namespace GimmeMillions.Domain.ML
{
    public class ModelMetrics
    {
        public double AreaUnderRocCurve { get; set; }
        public double Accuracy { get; set; }
        public double PositivePrecision { get; set; }
        public double PositiveRecall { get; set; }
        public double NegativePrecision { get; set; }
        public double NegativeRecall { get; set; }
        public double F1Score { get; set; }
        public double AreaUnderPrecisionRecallCurve { get; set; }
        public double Error { get; set; }

        public ModelMetrics()
        {

        }

        public ModelMetrics(BinaryClassificationMetrics metrics)
        {
            AreaUnderRocCurve = metrics.AreaUnderRocCurve;
            Accuracy = metrics.Accuracy;
            PositivePrecision = metrics.PositivePrecision;
            PositiveRecall = metrics.PositiveRecall;
            NegativeRecall = metrics.NegativeRecall;
            NegativePrecision = metrics.NegativePrecision;
            F1Score = metrics.F1Score;
            AreaUnderPrecisionRecallCurve = metrics.AreaUnderPrecisionRecallCurve;
        }

        public string ToString()
        {
            return $"AUC: {AreaUnderPrecisionRecallCurve}, Acc: {Accuracy}, PP: {PositivePrecision}, PR: {PositiveRecall}, NP: {NegativePrecision}, NR: {NegativeRecall}, F1: {F1Score}, Error: {Error}";
        }
    }
}
