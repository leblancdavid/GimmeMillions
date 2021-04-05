using Accord.Neuro.ActivationFunctions;
using System;

namespace GimmeMillions.Domain.ML.ActivationFunctions
{
    public class ExponentialLinearUnitFunction : IStochasticFunction
    {
        public double Alpha { get; private set; }
        public ExponentialLinearUnitFunction(double alpha = 0.5)
        {
            Alpha = alpha;
        }

        public double Derivative(double x)
        {
            return x >= 0 ? 1.0 : Function(x) + Alpha;
        }

        public double Derivative2(double y)
        {
            return y >= 0 ? 0.0 : Function(y) + Alpha;
        }

        public double Function(double x)
        {
            return x >= 0 ? x : Alpha * (Math.Pow(Math.E, x) - 1.0);
        }

        public double Generate(double x)
        {
            return x >= 0 ? x : Alpha * (Math.Pow(Math.E, x) - 1.0);
        }

        public double Generate2(double y)
        {
            return y >= 0 ? 1.0 : Alpha * (Math.Pow(Math.E, y) - 1.0);
        }
    }
}
