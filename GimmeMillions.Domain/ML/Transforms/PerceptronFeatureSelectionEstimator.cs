using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class PerceptronFeatureSelectionEstimator : IEstimator<ITransformer>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private int _rank;
        private int _featureSize;
        private int _iterations;
        private int _sliceSize;
        private MLContext _mLContext;
        private (int Index, double Weight)[] _featureContribution;

        public PerceptronFeatureSelectionEstimator(MLContext mLContext,
            int featureSize,
            int iterations,
            int rank = 1000,
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            _featureSize = featureSize;
            _iterations = iterations;
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _mLContext = mLContext;
            _rank = rank;
            _sliceSize = rank / 2;

            _featureContribution = new (int Index, double Weight)[_featureSize];
            for(int i = 0; i < _featureSize; ++i)
            {
                _featureContribution[i] = (i, 0.0);
            }
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
            
            var indicesToKeep = new int[_rank];
            Random rnd = new Random();
            for (int i = 0; i < _iterations; ++i)
            {
                RunIterationUpdateWeights(input, rnd);
            }
            var indicesToUse = _featureContribution.OrderByDescending(x => x.Weight)
                .Take(_rank).Select(x => x.Index).ToArray();
            return indicesToKeep;
        }

        private void RunIterationUpdateWeights(IDataView input, Random rnd)
        {
            var sortedContributions = _featureContribution.OrderBy(x => rnd.Next()).ToList();

            int numSlice = _featureSize / _sliceSize;
            for(int j = 0; j <= numSlice; ++j)
            {
                var remainingIndices = sortedContributions.Skip(j * _sliceSize);
                var indicesToUse = remainingIndices.Take(_sliceSize).Select(x => x.Index).ToArray();
                if(!indicesToUse.Any())
                {
                    indicesToUse = remainingIndices.Select(x => x.Index).ToArray();
                }
                var filter = new FeatureFilterTransform(_mLContext,
                    indicesToUse,
                    _inputColumnName, _outputColumnName);

                var filteredDataView = filter.Transform(input);
                var AP = _mLContext.BinaryClassification.Trainers.AveragedPerceptron().Fit(filteredDataView).Transform(filteredDataView);
                var results = _mLContext.BinaryClassification.EvaluateNonCalibrated(AP);

                foreach (var index in indicesToUse)
                {
                    _featureContribution[index].Weight += results.PositivePrecision;
                }

            }
            
        }
    }
}
