using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class RegressionFeatureVector
    {
        public float[] Features { get; set; }
        public float Label { get; set; }

        public RegressionFeatureVector(float[] input, float label)
        {
            Features = input;
            Label = label;
        }
    }
}
