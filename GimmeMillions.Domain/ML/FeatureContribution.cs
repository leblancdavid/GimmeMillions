using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class FeatureContribution
    {
        public float[] PositiveScore { get; set; }
        public float[] NegativeScore { get; set; }

        public void Build(IDataView data)
        {
            var features = data.GetColumn<float[]>("Features").ToArray();
            var labels = data.GetColumn<float>("Label").ToArray();

            if (features.Length == 0)
                return;

            int featureLength = features[0].Length;
            PositiveScore = new float[featureLength];
            NegativeScore = new float[featureLength];

            for(int i = 0; i < featureLength; ++i)
            {
                for(int j = 0; j < features.Length; ++j)
                {
                    if(labels[j] < 0)
                    {
                        NegativeScore[i] += features[j][i] * Math.Abs(labels[j]);
                    }
                    else
                    {
                        PositiveScore[i] += features[j][i] * Math.Abs(labels[j]);
                    }
                }
            }
        }

        public float GetPositiveFeatureProbability(int featureIndex)
        {
            if(featureIndex < 0 || featureIndex >= PositiveScore.Length)
            {
                return 0.0f;
            }

            return PositiveScore[featureIndex] / (PositiveScore[featureIndex] + NegativeScore[featureIndex]);
        }

        public float[] GetPositiveFeatureProbability()
        {
            var p = new float[PositiveScore.Length];
            for(int i = 0; i < p.Length; ++i)
            {
                p[i] = GetPositiveFeatureProbability(i);
            }
            return p;
        }

        public float GetNegativeFeatureProbability(int featureIndex)
        {
            if (featureIndex < 0 || featureIndex >= NegativeScore.Length)
            {
                return 0.0f;
            }

            return NegativeScore[featureIndex] / (PositiveScore[featureIndex] + NegativeScore[featureIndex]);
        }


    }
}
