using System;
using System.Linq;

using Microsoft.ML;
using Microsoft.ML.Data;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class RandomSelectionFeatureFilterEstimator : IEstimator<ITransformer>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private int _rank;
        private MLContext _mLContext;

        public RandomSelectionFeatureFilterEstimator(MLContext mLContext,
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
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();
            int featureLength = features[0].Length;
            var indicesToKeep = new int[featureLength];
            Random rnd = new Random();
            for (int i = 0; i < featureLength; ++i)
                indicesToKeep[i] = i;

            return indicesToKeep.OrderBy(x => rnd.Next()).Take(_rank).ToArray();
        }
    }
}