using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class FeatureFrequencyUsageFilterEstimator : IEstimator<ITransformer>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private int _rank;
        private MLContext _mLContext;

        public FeatureFrequencyUsageFilterEstimator(MLContext mLContext,
            string inputColumnName = "Features",
            string outputColumnName = "Label",
            int rank = 1000)
        {
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _mLContext = mLContext;
            _rank = rank;
        }

        public ITransformer Fit(IDataView input)
        {
            return new FeatureFilterTransform(_mLContext, GetFeatureSelectionIndices(input), _inputColumnName, _outputColumnName);
        }

        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            return inputSchema;
        }

        private int[] GetFeatureSelectionIndices(IDataView input)
        {
            var differences = GetFeatureUsage(input);
            var orderedDifferences = differences.OrderByDescending(x => x.Usage).ToList();
            var indicesToKeep = orderedDifferences.Skip(100).Take(_rank).Select(x => x.Index);

            return indicesToKeep.ToArray();
        }

        private (float Usage, int Index)[] GetFeatureUsage(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();

            if (features.Length == 0)
                throw new Exception($"Input features for the FeatureSelectorEstimator contains no elements");

            int featureLength = features[0].Length;
            var p = new (float Usage, int Index)[featureLength];

            for (int i = 0; i < featureLength; ++i)
            {
                p[i].Index = i;
                for (int j = 0; j < features.Length; ++j)
                {
                    p[i].Usage += Math.Abs(features[j][i]);
                }
            }

            return p;
        }
            
    }
}
