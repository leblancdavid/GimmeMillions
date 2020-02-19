using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class TrainingResult
    {
        public double Accuracy { get; set; }
        public double AverageRegressionError { get; set; }
    }
}
