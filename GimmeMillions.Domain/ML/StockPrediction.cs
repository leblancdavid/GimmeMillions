using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class StockPrediction
    {
        public double PercentChange { get; set; }
        public double Confidence { get; set; }
    }
}
