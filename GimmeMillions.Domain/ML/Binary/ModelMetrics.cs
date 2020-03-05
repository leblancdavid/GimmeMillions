using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Binary
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
    }
}
