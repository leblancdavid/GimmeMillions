using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class KnnBruteForceEstimator : IEstimator<KnnBruteForceTransform>
    {
        private string _inputColumnName;
        private MLContext _mLContext;

        public KnnBruteForceEstimator(MLContext mLContext,
            string inputColumnName = "Features")
        {
            _inputColumnName = inputColumnName;
            _mLContext = mLContext;
        }
        public KnnBruteForceTransform Fit(IDataView input)
        {
            return new KnnBruteForceTransform(_mLContext, input);
        }

        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            return inputSchema;
        }
    }
}
