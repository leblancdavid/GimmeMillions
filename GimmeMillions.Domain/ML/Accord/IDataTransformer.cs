using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Accord
{
    public interface IDataTransformer
    {
        bool IsFitted { get; }
        double[][] Transform(double[][] input);
        double[] Transform(double[] input);
        void Fit(double[][] input);
        bool Load(string fileName);
        void Save(string fileName);
    }
}
