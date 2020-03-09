using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class MaxVarianceFeatureFilterEstimator : IEstimator<ITransformer>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private int _rank;
        private MLContext _mLContext;

        public MaxVarianceFeatureFilterEstimator(MLContext mLContext,
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
            var differences = GetAbsoluteDifference(input);
            var orderedDifferences = differences.OrderByDescending(x => x.FeatureDifference).ToArray();
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
            var positiveAvg = new float[featureLength];
            var negativeAvg = new float[featureLength];
            var positiveVar = new float[featureLength];
            var negativeVar = new float[featureLength];
            float negativeTotal = labels.Sum(x => !x ? 1.0f : 0.0f),
                positiveTotal = labels.Sum(x => x ? 1.0f : 0.0f);

            for (int i = 0; i < featureLength; ++i)
            {
                //Initialize with a 1
                positiveAvg[i] = 0.0f;
                negativeAvg[i] = 0.0f;
                for (int j = 0; j < features.Length; ++j)
                {
                    if (labels[j])
                    {
                        positiveAvg[i] += features[j][i];
                    }
                    else
                    {
                        negativeAvg[i] += features[j][i];
                    }
                }
            }

            for (int i = 0; i < featureLength; ++i)
            {
                positiveAvg[i] /= positiveTotal;
                negativeAvg[i] /= negativeTotal;
                
                positiveVar[i] = 0.0f;
                negativeVar[i] = 0.0f;

                for (int j = 0; j < features.Length; ++j)
                {
                    if (labels[j])
                    {
                        positiveVar[i] += Math.Abs(features[j][i] - positiveAvg[i]);
                    }
                    else
                    {
                        negativeVar[i] += Math.Abs(features[j][i] - negativeAvg[i]);
                    }
                }
            }

            var p = new (float FeatureDifference, int Index)[positiveAvg.Length];
            for (int i = 0; i < p.Length; ++i)
            {
                p[i] = (Math.Abs((negativeVar[i] / negativeTotal) + (positiveVar[i] / positiveTotal)), i);
                //p[i] = ((positiveVar[i] / positiveTotal) - (negativeVar[i] / negativeTotal), i);
            }
            return p;
        }
    }
}
