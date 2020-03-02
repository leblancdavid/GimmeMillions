using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class BinaryClassificationFeatureVector
    {
        public float[] Features { get; set; }
        public bool Label { get; set; }

        public BinaryClassificationFeatureVector(float[] input, bool label)
        {
            Features = input;
            Label = label;
        }
    }
}
