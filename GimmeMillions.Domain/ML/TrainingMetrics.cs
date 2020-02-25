using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML
{
    public class TrainingMetrics<MlMetrics>
    {
        public double AveragePositiveProbability { get; private set; }
        public double StdevPositiveProbability { get; private set; }
        public double AverageNegativeProbability { get; private set; }
        public double StdevNegativePProbability { get; private set; }
        public List<(double P0, double P1, double Accuracy, int Samples)> TruePositiveDistribution { get; set; }
        public double AverageTruePositiveAccuracy
        {
            get
            {
                return TruePositiveDistribution.Sum(x => x.Accuracy * (double)x.Samples) / (double)TruePositiveDistribution.Sum(x => x.Samples);
            }
        }

        public MlMetrics Metrics { get; private set; }

        public TrainingMetrics(MlMetrics mlMetrics)
        {
            Metrics = mlMetrics;
        }


        public void ComputeStatistics(IDataView predictedData, double s0 = -3.0, double s1 = 3.0, double sigmaIncrement = 1.0)
        {
            AveragePositiveProbability = 0.0;
            StdevPositiveProbability = 0.0;
            AverageNegativeProbability = 0.0;
            StdevNegativePProbability = 0.0;

            var probabilityColumn = predictedData.GetColumn<float>("Probability").ToArray();

            var posProbs = probabilityColumn.Where(x => x >= 0.5f);
            var negProbs = probabilityColumn.Where(x => x < 0.5f);

            AveragePositiveProbability = posProbs.Average();
            StdevPositiveProbability = Math.Sqrt(posProbs.Sum(x => Math.Pow(x - AveragePositiveProbability, 2)) / posProbs.Count());
            AverageNegativeProbability = negProbs.Average();
            StdevNegativePProbability = Math.Sqrt(negProbs.Sum(x => Math.Pow(x - AverageNegativeProbability, 2)) / negProbs.Count());

            TruePositiveDistribution = new List<(double Sigma0, double Sigma1, double Accuracy, int Samples)>();
            for (double sigma = s0; sigma < s1; sigma += sigmaIncrement)
            {
                var results = GetTruePositiveAccuracy(predictedData, sigma, sigma + sigmaIncrement);
                if(results.Samples > 0)
                {
                    TruePositiveDistribution.Add(results);
                }
            }
        }

        public (double P0, double P1, double Accuracy, int Samples) GetTruePositiveAccuracy(IDataView predictedData, double lowerSigma, double upperSigma)
        {
            if(lowerSigma > upperSigma)
            {
                return (0.0, 0.0, 0.0, 0);
            }

            double lowerBound = AveragePositiveProbability + (lowerSigma * StdevPositiveProbability);
            double upperBound = AveragePositiveProbability + (upperSigma * StdevPositiveProbability);

            var probabilityColumn = predictedData.GetColumn<float>("Probability").ToArray();
            var labelColumn = predictedData.GetColumn<bool>("Label").ToArray();

            int totalCount = 0;
            double accuracy = 0.0;
            for(int i = 0; i < probabilityColumn.Length; ++i)
            {
                if(probabilityColumn[i] >= lowerBound && probabilityColumn[i] <= upperBound)
                {
                    totalCount++;
                    if(labelColumn[i])
                    {
                        accuracy++;
                    }
                }
            }

            if(totalCount > 0)
            {
                accuracy /= (double)totalCount;
                return (lowerBound, upperBound, accuracy, totalCount);
            }

            return (lowerBound, upperBound, 0.0, 0);
        }

        public (double P0, double P1, double Accuracy, int Samples) GetTrueNegativeAccuracy(IDataView predictedData, double lowerSigma, double upperSigma)
        {
            if (lowerSigma > upperSigma)
            {
                return (0.0, 0.0, 0.0, 0);
            }

            double lowerBound = AverageNegativeProbability + (lowerSigma * StdevNegativePProbability);
            double upperBound = AverageNegativeProbability + (lowerSigma * StdevNegativePProbability);

            var probabilityColumn = predictedData.GetColumn<float>("Probability").ToArray();
            var labelColumn = predictedData.GetColumn<bool>("Label").ToArray();

            int totalCount = 0;
            double accuracy = 0.0;
            for (int i = 0; i < probabilityColumn.Length; ++i)
            {
                if (probabilityColumn[i] >= lowerBound && probabilityColumn[i] <= upperBound)
                {
                    totalCount++;
                    if (!labelColumn[i])
                    {
                        accuracy++;
                    }
                }
            }

            if (totalCount > 0)
            {
                accuracy /= (double)totalCount;
                return (lowerBound, upperBound, accuracy, totalCount);
            }

            return (lowerBound, upperBound, 0.0, 0);
        }
    }
}
