using Accord.Neuro.ActivationFunctions;

namespace GimmeMillions.Domain.ML.ActivationFunctions
{
    public class RectifiedLinearUnitFunction : IStochasticFunction
    {
        public double Derivative(double x)
        {
            return x >= 0 ? 1.0 : 0.0;
        }

        public double Derivative2(double y)
        {
            return y >= 0 ? 1.0 : 0.0;
        }

        public double Function(double x)
        {
            return x >= 0 ? x : 0.0;
        }

        public double Generate(double x)
        {
            return x >= 0 ? x : 0.0;
        }

        public double Generate2(double y)
        {
            return y >= 0 ? y : 0.0;
        }
    }
}
