using System;
using System.Linq;

namespace GimmeMillions.Domain.ML.Accord
{
    public class FilterFeaturesDataTransformer : IDataTransformer
    {
        private int[] _indicesToKeep;
        private int _numIndices;
        public bool IsFitted { get; private set; }

        public FilterFeaturesDataTransformer(int numIndices)
        {
            _numIndices = numIndices;
            IsFitted = false;
        }

        public FilterFeaturesDataTransformer(int[] indicesToKeep)
        {
            _indicesToKeep = indicesToKeep;
            _numIndices = indicesToKeep.Length;
            IsFitted = true;
        }


        public void Fit(double[][] input)
        {
            var variance = GetAbsoluteVariance(input).OrderByDescending(x => x.Variance).ToArray();
            if(variance.Length < _numIndices)
            {
                _numIndices = variance.Length;
            }
            _indicesToKeep = new int[_numIndices];
            for(int i = 0; i < _numIndices; ++i)
            {
                _indicesToKeep[i] = variance[i].Index;
            }
            IsFitted = true;
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

        private (double Variance, int Index)[] GetAbsoluteVariance(double[][] inputs)
        {

            if (!inputs.Any())
                throw new Exception($"Input features for the FeatureSelectorEstimator contains no elements");

            int featureLength = inputs.First().Length;
            var average = new double[featureLength];

            for (int i = 0; i < featureLength; ++i)
            {
                average[i] = 0.0;
                for (int j = 0; j < inputs.Length; ++j)
                {
                    average[i] += inputs[j][i];
                }
            }

            var variance = new (double Variance, int Index)[average.Length];
            for (int i = 0; i < featureLength; ++i)
            {
                average[i] /= inputs.Length;

                for (int j = 0; j < inputs.Length; ++j)
                {
                    variance[i].Variance += Math.Pow(inputs[j][i] - average[i], 2.0);
                }

                variance[i].Variance = Math.Sqrt(variance[i].Variance / inputs.Length);
                variance[i].Index = i;
            }

            return variance;
        }
    }
}
