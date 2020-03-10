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
            _sliceSize = _featureSize / 2;

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
            var dataNormalizer = _mLContext.Transforms.NormalizeMinMax("Features").Fit(input);
            var normalizedData = dataNormalizer.Transform(input);

            Random rnd = new Random();
            for (int i = 0; i < _iterations; ++i)
            {
                RunIterationUpdateWeights(normalizedData, rnd);
            }
            var sortedFeatures = _featureContribution.OrderByDescending(x => x.Weight).ToList();
            var indicesToUse = sortedFeatures.Take(_rank).Select(x => x.Index).ToArray();
            return indicesToUse;
            //double bestPrecision = 0.0;
            //int[] bestIndices = null;
            //for (int i = 0; i < _iterations; ++i)
            //{
            //    var currentIndices = _featureContribution.OrderBy(x => rnd.Next())
            //        .Take(_rank).Select(x => x.Index).ToArray();

            //    var filter = new FeatureFilterTransform(_mLContext,
            //        currentIndices,
            //        _inputColumnName, _outputColumnName);

            //    var filteredDataView = filter.Transform(input);
            //    var results = _mLContext.BinaryClassification.EvaluateNonCalibrated(
            //        _mLContext.BinaryClassification.Trainers.LinearSvm()
            //            .Fit(filteredDataView).Transform(filteredDataView));

            //    if (results.Accuracy > bestPrecision)
            //    {
            //        Console.WriteLine($"Accuracy: {results.Accuracy}, PP: {results.PositivePrecision}, PR: {results.PositiveRecall}");
            //        bestIndices = currentIndices;
            //        bestPrecision = results.Accuracy;

            //    }
            //}

            //return bestIndices;
        }

        private void RunIterationUpdateWeights(IDataView input, Random rnd)
        {
            var sortedContributions = _featureContribution.OrderBy(x => rnd.Next()).ToList();

            int numSlice = _featureSize / _sliceSize;
            for(int j = 0; j <= numSlice; ++j)
            {
                var remainingIndices = sortedContributions.Skip(j * _sliceSize - 1);
                var indicesToUse = remainingIndices.Take(_sliceSize).Select(x => x.Index).ToArray();
                if(indicesToUse.Count() != _sliceSize)
                {
                    indicesToUse = remainingIndices.Select(x => x.Index).ToArray();
                }
                var filter = new FeatureFilterTransform(_mLContext,
                    indicesToUse,
                    _inputColumnName, _outputColumnName);

                var filteredDataView = filter.Transform(input); 
                //var AP = _mLContext.BinaryClassification.Trainers.FastTree(numberOfLeaves: 2, numberOfTrees: 10).Fit(filteredDataView).Transform(filteredDataView);
                var AP = _mLContext.BinaryClassification.Trainers.LinearSvm(numberOfIterations: 10).Fit(filteredDataView).Transform(filteredDataView);
                var results = _mLContext.BinaryClassification.EvaluateNonCalibrated(AP);

                foreach (var index in indicesToUse)
                {
                    _featureContribution[index].Weight += results.Accuracy;
                }

            }
            
        }
    }
}
