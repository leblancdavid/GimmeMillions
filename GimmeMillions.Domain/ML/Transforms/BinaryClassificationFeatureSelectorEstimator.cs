﻿using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class BinaryClassificationFeatureSelectorEstimator : IEstimator<ITransformer>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private float _lowerStdev;
        private float _upperStdev;
        private bool _inclusive;
        private MLContext _mLContext;

        public BinaryClassificationFeatureSelectorEstimator(MLContext mLContext, 
            string inputColumnName = "Features", 
            string outputColumnName = "Label",
            float lowerStdev = 0.0f,
            float upperStdev = 0.0f,
            bool inclusive = true)
        {
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _lowerStdev = lowerStdev;
            _upperStdev = upperStdev;
            _mLContext = mLContext;
            _inclusive = inclusive;
        }

        public ITransformer Fit(IDataView input)
        {
            return new BinaryClassificationFeatureSelectorTransform(_mLContext, GetFeatureSelectionIndices(input), _inputColumnName, _outputColumnName);
        }

        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            return inputSchema;
        }

        private int[] GetFeatureSelectionIndices(IDataView input)
        {
            var probabilities = GetPositiveProbability(input);

            var averageP = probabilities.Average();
            var stdDev = (float)Math.Sqrt(probabilities.Select(x => Math.Pow(x - averageP, 2.0)).Average());

            float lower = stdDev * _lowerStdev + averageP;
            float upper = stdDev * _upperStdev + averageP;
            var indicesToKeep = new List<int>();
            for(int i = 0; i < probabilities.Length; ++i)
            {
                if((!_inclusive && (probabilities[i] <= lower || probabilities[i] >= upper)) ||
                    (_inclusive && (probabilities[i] >= lower && probabilities[i] <= upper)))
                {
                    indicesToKeep.Add(i);
                }
            }

            return indicesToKeep.ToArray();
        }

        private float[] GetPositiveProbability(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();

            if (features.Length == 0)
                throw new Exception($"Input features for the FeatureSelectorEstimator contains no elements");


            int featureLength = features[0].Length;
            var positiveScore = new float[featureLength];
            var negativeScore = new float[featureLength];

            for (int i = 0; i < featureLength; ++i)
            {
                //Initialize with a 1
                positiveScore[i] = 1.0f;
                negativeScore[i] = 1.0f;
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

            var p = new float[positiveScore.Length];
            for (int i = 0; i < p.Length; ++i)
            {
                float total = positiveScore[i] + negativeScore[i];
                if (total < 0.0001f)
                {
                    p[i] = 0.0f;
                }
                else
                {
                    p[i] = positiveScore[i] / (positiveScore[i] + negativeScore[i]);
                }
            }
            return p;
        }

    }
}
