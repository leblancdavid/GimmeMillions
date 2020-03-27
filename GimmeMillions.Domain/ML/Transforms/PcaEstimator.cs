using Accord.Statistics.Analysis;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class PcaEstimator : IEstimator<PcaTransform>
    {
        private string _inputColumnName;
        private string _outputColumnName;
        private int _rank;
        private MLContext _mLContext;

        public PcaEstimator(MLContext mLContext,
            string inputColumnName = "Features",
            string outputColumnName = "Label",
            int rank = 1000)
        {
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _mLContext = mLContext;
            _rank = rank;
        }

        public PcaTransform Fit(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName)
                .Select(x => Array.ConvertAll(x, y => (double)y)).ToArray();

            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true,
                NumberOfOutputs = _rank
            };

            pca.Learn(features);

            return new PcaTransform(_mLContext, pca, _inputColumnName, _outputColumnName);

        }

        public SchemaShape GetOutputSchema(SchemaShape inputSchema)
        {
            return inputSchema;
        }
    }
}
