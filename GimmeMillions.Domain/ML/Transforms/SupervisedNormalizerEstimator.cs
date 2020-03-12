using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class SupervisedNormalizerEstimator : IEstimator<ITransformer>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private MLContext _mLContext;

        public SupervisedNormalizerEstimator(MLContext mLContext,
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _mLContext = mLContext;
        }

        public ITransformer Fit(IDataView input)
        {
            return new SupervisedNormalizerTransform(_mLContext, ComputeDataStatistics(input), _inputColumnName, _outputColumnName);
        }

        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            return inputSchema;
        }

        private (float pMean, float pStdev, float nMean, float nStdev)[] ComputeDataStatistics(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();

            if (features.Length == 0)
                throw new Exception($"Input features for the FeatureSelectorEstimator contains no elements");


            int featureLength = features[0].Length;
            var statistics = new (float pMean, float pStdev, float nMean, float nStdev)[featureLength];
            float negativeTotal = labels.Sum(x => !x ? 1.0f : 0.0f),
                positiveTotal = labels.Sum(x => x ? 1.0f : 0.0f);

            for (int i = 0; i < featureLength; ++i)
            {
                for (int j = 0; j < features.Length; ++j)
                {
                    if (labels[j])
                    {
                        statistics[i].pMean += features[j][i];
                    }
                    else
                    {
                        statistics[i].nMean += features[j][i];
                    }
                }
            }

            for (int i = 0; i < featureLength; ++i)
            {
                statistics[i].pMean /= positiveTotal;
                statistics[i].nMean /= negativeTotal;

                for (int j = 0; j < features.Length; ++j)
                {
                    if (labels[j])
                    {
                        statistics[i].pStdev += (float)Math.Pow(features[j][i] - statistics[i].pMean, 2.0);
                    }
                    else
                    {
                        statistics[i].nStdev += (float)Math.Pow(features[j][i] - statistics[i].nMean, 2.0);
                    }
                }

                statistics[i].pStdev = (float)Math.Sqrt(statistics[i].pStdev / positiveTotal);
                statistics[i].nStdev = (float)Math.Sqrt(statistics[i].nStdev / negativeTotal);
            }

            return statistics;
        }
    }
}
