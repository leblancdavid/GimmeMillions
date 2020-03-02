using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class BinaryClassificationFeatureSelectorTransform : ITransformer
    {
        private int[] _featureIndices;
        private string _inputColumnName;
        private string _outputColumnName;
        private MLContext _mLContext;

        public bool IsRowToRowMapper => true;

        public BinaryClassificationFeatureSelectorTransform(MLContext mLContext,
            int[] featureIndices,
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            _mLContext = mLContext;
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _featureIndices = featureIndices;
        }

        public DataViewSchema GetOutputSchema(DataViewSchema inputSchema)
        {
            return inputSchema;
        }

        public IRowToRowMapper GetRowToRowMapper(DataViewSchema inputSchema)
        {
            throw new NotImplementedException();
        }

        public void Save(ModelSaveContext ctx)
        {
            throw new NotImplementedException();
        }

        public IDataView Transform(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();

            var output = new List<BinaryClassificationFeatureVector>();
            for(int i = 0; i < features.Length; ++i)
            {
                var filtered = new float[_featureIndices.Length];
                for(int j = 0; j < _featureIndices.Length; ++j)
                {
                    filtered[j] = features[i][_featureIndices[j]];
                }
                output.Add(new BinaryClassificationFeatureVector(filtered, labels[i]));
            }

            int featureDimension = _featureIndices.Length;
            var definedSchema = SchemaDefinition.Create(typeof(BinaryClassificationFeatureVector));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);
            return _mLContext.Data.LoadFromEnumerable(output, definedSchema);
        }
    }
}
