using CSharpFunctionalExtensions;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class FeatureFilterRegressionTransform : ITransformer
    {
        private int[] _featureIndices;
        private string _inputColumnName;
        private string _outputColumnName;
        private MLContext _mLContext;

        public bool IsRowToRowMapper => true;

        public FeatureFilterRegressionTransform(MLContext mLContext,
            int[] featureIndices,
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            _mLContext = mLContext;
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _featureIndices = featureIndices;
        }

        public static Result<FeatureFilterRegressionTransform> LoadFromFile(string fileName,
            MLContext mLContext, 
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            if (!File.Exists(fileName))
            {
                return Result.Failure<FeatureFilterRegressionTransform>($"BinaryClassificationFeatureSelectorTransform model named {fileName} could not be found");
            }
            var json = File.ReadAllText(fileName);
            return Result.Ok(new FeatureFilterRegressionTransform(mLContext, 
                JsonConvert.DeserializeObject<int[]>(json),
                inputColumnName, outputColumnName));
        }

        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(_featureIndices, Formatting.Indented));
        }

        public DataViewSchema GetOutputSchema(DataViewSchema inputSchema)
        {
            var annotationBuilder = new DataViewSchema.Annotations.Builder();
            annotationBuilder.AddPrimitiveValue<bool>("IsNormalized", BooleanDataViewType.Instance, true);
            var schemaBuilder = new DataViewSchema.Builder();
            schemaBuilder.AddColumn(_inputColumnName, new VectorDataViewType((
                (VectorDataViewType)inputSchema[_inputColumnName].Type).ItemType, _featureIndices.Length), 
                annotationBuilder.ToAnnotations());
            schemaBuilder.AddColumn(_outputColumnName, inputSchema[_outputColumnName].Type);
            schemaBuilder.AddColumn("Value", inputSchema["Value"].Type);
            schemaBuilder.AddColumn("DayOfTheWeek", inputSchema["DayOfTheWeek"].Type);
            schemaBuilder.AddColumn("Month", inputSchema["Month"].Type);
            var schema = schemaBuilder.ToSchema();
            return schema;
        }

        public IRowToRowMapper GetRowToRowMapper(DataViewSchema inputSchema)
        {
            throw new NotImplementedException();
        }

        public void Save(ModelSaveContext ctx)
        {
            //Not sure how I'm suppose to go about implementing this!!!
            //ModelSaveContext is just an empty class with a bunch of internals that I can't use
            throw new NotImplementedException();
        }

        public IDataView Transform(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName).ToArray();
            var labels = input.GetColumn<float>(_outputColumnName).ToArray();
            var dayOfTheWeek = input.GetColumn<float>("DayOfTheWeek").ToArray();
            var month = input.GetColumn<float>("Month").ToArray();

            var output = new List<StockRegressionDataFeature>();
            for(int i = 0; i < features.Length; ++i)
            {
                var filtered = new float[_featureIndices.Length];
                for(int j = 0; j < _featureIndices.Length; ++j)
                {
                    filtered[j] = features[i][_featureIndices[j]];
                }
                output.Add(new StockRegressionDataFeature(filtered, labels[i], dayOfTheWeek[i], month[i]));
            }

            int featureDimension = _featureIndices.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRegressionDataFeature));
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);
            return _mLContext.Data.LoadFromEnumerable(output, definedSchema);
        }
    }
}
