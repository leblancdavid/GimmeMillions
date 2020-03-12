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
    public class SupervisedNormalizerTransform : ITransformer
    {
        private (float pMean, float pStdev, float nMean, float nStdev)[] _statistics;
        private string _inputColumnName;
        private string _outputColumnName;
        private MLContext _mLContext;

        public bool IsRowToRowMapper => true;

        public SupervisedNormalizerTransform(MLContext mLContext,
            (float pMean, float pStdev, float nMean, float nStdev)[] statistics,
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            _mLContext = mLContext;
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _statistics = statistics;
        }

        public static Result<SupervisedNormalizerTransform> LoadFromFile(string fileName,
            MLContext mLContext,
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            if (!File.Exists(fileName))
            {
                return Result.Failure<SupervisedNormalizerTransform>($"SupervisedNormalizerTransform model named {fileName} could not be found");
            }
            var json = File.ReadAllText(fileName);
            return Result.Ok(new SupervisedNormalizerTransform(mLContext,
                JsonConvert.DeserializeObject<(float pMean, float pStdev, float nMean, float nStdev)[]>(json),
                inputColumnName, outputColumnName));
        }

        public void SaveToFile(string fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(_statistics, Formatting.Indented));
        }

        public DataViewSchema GetOutputSchema(DataViewSchema inputSchema)
        {
            var annotationBuilder = new DataViewSchema.Annotations.Builder();
            annotationBuilder.AddPrimitiveValue<bool>("IsNormalized", BooleanDataViewType.Instance, true);
            var schemaBuilder = new DataViewSchema.Builder();
            schemaBuilder.AddColumn(_inputColumnName, new VectorDataViewType((
                (VectorDataViewType)inputSchema[_inputColumnName].Type).ItemType, _statistics.Length),
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
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();
            var values = input.GetColumn<float>("Value").ToArray();
            var dayOfTheWeek = input.GetColumn<float>("DayOfTheWeek").ToArray();
            var month = input.GetColumn<float>("Month").ToArray();

            var output = new List<StockRiseDataFeature>();
            for (int i = 0; i < features.Length; ++i)
            {
                var normalized = new float[_statistics.Length];
                for (int j = 0; j < _statistics.Length; ++j)
                {
                    float pos = (features[i][j] - _statistics[j].pMean) / _statistics[j].pStdev;
                    float neg = (features[i][j] - _statistics[j].nMean) / _statistics[j].nStdev;
                    if(float.IsNaN(pos))
                    {
                        pos = 0.0f;
                    }
                    if(float.IsNaN(neg))
                    {
                        neg = 0.0f;
                    }
                    normalized[j] = Math.Abs(pos) - Math.Abs(neg);
                }
                output.Add(new StockRiseDataFeature(normalized, labels[i], values[i], dayOfTheWeek[i], month[i]));
            }

            int featureDimension = _statistics.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);
            return _mLContext.Data.LoadFromEnumerable(output, definedSchema);
        }
    }
}
