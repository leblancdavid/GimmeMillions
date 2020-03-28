using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Regression
{
    public class MLRegressionMetrics
    {
        //
        // Summary:
        //     Gets the absolute loss of the model.
        //
        // Remarks:
        //     The absolute loss is defined as L1 = (1/m) * sum( abs( yi - y'i)) where m is
        //     the number of instances in the test set. y'i are the predicted labels for each
        //     instance. yi are the correct labels of each instance.
        public double MeanAbsoluteError { get; set; }
        //
        // Summary:
        //     Gets the squared loss of the model.
        //
        // Remarks:
        //     The squared loss is defined as L2 = (1/m) * sum(( yi - y'i)^2) where m is the
        //     number of instances in the test set. y'i are the predicted labels for each instance.
        //     yi are the correct labels of each instance.
        public double MeanSquaredError { get; set; }
        //
        // Summary:
        //     Gets the root mean square loss (or RMS) which is the square root of the L2 loss.
        public double RootMeanSquaredError { get; set; }
        //
        // Summary:
        //     Gets the result of user defined loss function.
        //
        // Remarks:
        //     This is the average of a loss function defined by the user, computed over all
        //     the instances in the test set.
        public double LossFunction { get; set; }
        //
        // Summary:
        //     Gets the R squared value of the model, which is also known as the coefficient
        //     of determination​.
        public double RSquared { get; set;  }
    }
}
