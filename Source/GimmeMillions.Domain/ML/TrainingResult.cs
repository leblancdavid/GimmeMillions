using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class TrainingResult
    {
        public double TrainingAccuracy { get; set; }
        public double TrainingError { get; set; }
        public double ValidationAccuracy { get; set; }
        public double ValidationError { get; set; }
    }
}
