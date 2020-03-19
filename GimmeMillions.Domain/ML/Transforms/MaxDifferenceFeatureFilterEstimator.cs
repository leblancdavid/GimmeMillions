using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class MaxDifferenceFeatureFilterEstimator : IEstimator<FeatureFilterTransform>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private int _rank;
        private bool _positiveSort;
        private MLContext _mLContext;

        public MaxDifferenceFeatureFilterEstimator(MLContext mLContext,
            string inputColumnName = "Features",
            string outputColumnName = "Label",
            int rank = 1000,
            bool positiveSort = true)
        {
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _mLContext = mLContext;
            _rank = rank;
            _positiveSort = positiveSort;
        }

        public FeatureFilterTransform Fit(IDataView input)
        {
            return new FeatureFilterTransform(_mLContext, GetFeatureSelectionIndices(input), _inputColumnName, _outputColumnName);
        }

        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            return inputSchema;
        }

        private int[] GetFeatureSelectionIndices(IDataView input)
        {
            var differences = GetAbsoluteDifference(input);
            var orderedDifferences = differences.OrderByDescending(x => x.FeatureDifference).ToList();
            //var orderedDifferences = differences.OrderBy(x => x.FeatureDifference);
            var indicesToKeep = orderedDifferences.Take(_rank).Select(x => x.Index);
          
            return indicesToKeep.ToArray();
        }

        private (float FeatureDifference, int Index)[] GetAbsoluteDifference(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();

            if (features.Length == 0)
                throw new Exception($"Input features for the FeatureSelectorEstimator contains no elements");


            int featureLength = features[0].Length;
            var positiveScore = new float[featureLength];
            var negativeScore = new float[featureLength];
            float negativeTotal = labels.Sum(x => !x ? 1.0f: 0.0f), 
                positiveTotal = labels.Sum(x => x ? 1.0f : 0.0f);

            for (int i = 0; i < featureLength; ++i)
            {
                for (int j = 0; j < features.Length; ++j)
                {
                    if (labels[j])
                    {
                        positiveScore[i] += features[j][i];
                    }
                    else
                    {
                        negativeScore[i] += features[j][i];
                    }
                }
            }

            var p = new (float FeatureDifference, int Index)[positiveScore.Length];
            for (int i = 0; i < p.Length; ++i)
            {
                if (_positiveSort)
                {
                    p[i] = ((positiveScore[i]) - (negativeScore[i]), i);
                    //p[i] = ((positiveScore[i] / positiveTotal) - (negativeScore[i] / negativeTotal), i);
                }
                else
                {
                    p[i] = ((negativeScore[i]) - (positiveScore[i]), i);
                   // p[i] = ((negativeScore[i] / negativeTotal) - (positiveScore[i] / positiveTotal), i);
                }
                //p[i] = (Math.Abs((negativeScore[i] / negativeTotal) - (positiveScore[i] / positiveTotal)), i);
            }
            return p;
        }
    }
}
