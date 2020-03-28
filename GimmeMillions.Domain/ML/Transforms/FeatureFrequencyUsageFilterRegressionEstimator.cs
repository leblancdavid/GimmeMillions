using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class FeatureFrequencyUsageFilterRegressionEstimator : IEstimator<FeatureFilterRegressionTransform>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private int _rank;
        private int _skip;
        private MLContext _mLContext;

        public FeatureFrequencyUsageFilterRegressionEstimator(MLContext mLContext,
            string inputColumnName = "Features",
            string outputColumnName = "Label",
            int rank = 1000,
            int skip = 100)
        {
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _mLContext = mLContext;
            _skip = skip;
            _rank = rank;
        }

        public FeatureFilterRegressionTransform Fit(IDataView input)
        {
            return new FeatureFilterRegressionTransform(_mLContext, GetFeatureSelectionIndices(input), _inputColumnName, _outputColumnName);
        }

        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            return inputSchema;
        }

        private int[] GetFeatureSelectionIndices(IDataView input)
        {
            var differences = GetFeatureUsage(input);
            var orderedDifferences = differences.OrderByDescending(x => x.Usage).ToList();
            IEnumerable<int> indicesToKeep;
            if(_skip > 0)
                indicesToKeep = orderedDifferences.Skip(_skip).Take(_rank).Select(x => x.Index);
            else
                indicesToKeep = orderedDifferences.Take(_rank).Select(x => x.Index);

            return indicesToKeep.ToArray();
        }

        private (float Usage, int Index)[] GetFeatureUsage(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();

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
