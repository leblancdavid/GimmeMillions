using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Accord
{
    public class SupervisedNormalizationDataTransformer : IDataTransformer
    {
        private (double pMean, double pStdev, double nMean, double nStdev)[] _statistics;
        public SupervisedNormalizationDataTransformer((double pMean, double pStdev, double nMean, double nStdev)[] statistics)
        {
            _statistics = statistics;
        }
        public bool Load(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName)
        {
            throw new NotImplementedException();
        }

        public double[][] Transform(double[][] input)
        {
            var output = new double[input.Length][];
            for (int i = 0; i < input.Length; ++i)
            {
                output[i] = Transform(input[i]);
            }
            return output;
        }

        public double[] Transform(double[] input)
        {
            var normalized = new double[_statistics.Length];
            for (int j = 0; j < _statistics.Length; ++j)
            {
                double pos = (input[j] - _statistics[j].pMean) / _statistics[j].pStdev;
                double neg = (input[j] - _statistics[j].nMean) / _statistics[j].nStdev;
                if (double.IsNaN(pos))
                {
                    pos = 0.0;
                }
                if (double.IsNaN(neg))
                {
                    neg = 0.0;
                }
                normalized[j] = Math.Abs(pos) - Math.Abs(neg);
            }
            return normalized;
        }
    }
}
