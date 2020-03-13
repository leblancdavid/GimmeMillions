using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Accord
{
    public class FilterFeaturesDataTransformer : IDataTransformer
    {
        private int[] _indicesToKeep;
        public FilterFeaturesDataTransformer(int[] indicesToKeep)
        {
            _indicesToKeep = indicesToKeep;
        }

        public bool Load(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName)
        {
            throw new NotImplementedException();
        }

        public double[][] Transform(double[][] input)
        {
            var filteredFeatures = new double[input.Length][];

            for (int i = 0; i < input.Length; ++i)
            {
                filteredFeatures[i] = Transform(input[i]);
            }

            return filteredFeatures;
        }

        public double[] Transform(double[] input)
        {
            var filteredFeature = new double[_indicesToKeep.Length];
            for (int j = 0; j < _indicesToKeep.Length; ++j)
            {
                filteredFeature[j] = input[_indicesToKeep[j]];
            }

            return filteredFeature;
        }
    }
}
