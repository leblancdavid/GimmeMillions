using Accord.Statistics.Analysis;
using CSharpFunctionalExtensions;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class PcaTransform : ITransformer
    {
        private PrincipalComponentAnalysis _pca;
        private string _inputColumnName;
        private string _outputColumnName;
        public bool IsRowToRowMapper => true;
        public PcaTransform(PrincipalComponentAnalysis pca,
            string inputColumnName = "Features",
            string outputColumnName = "Label")
        {
            _pca = pca;
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            // _pca.NumberOfOutputs = _pcaRank;
        }

        public static Result<PcaTransform> LoadFromFile(string fileName,
           MLContext mLContext,
           string inputColumnName = "Features",
           string outputColumnName = "Label")
        {

            throw new NotImplementedException();

            //if (!File.Exists(fileName))
            //{
            //    return Result.Failure<PcaTransform>($"BinaryClassificationFeatureSelectorTransform model named {fileName} could not be found");
            //}
            //var json = File.ReadAllText(fileName);
            //return Result.Ok(new FeatureFilterTransform(mLContext,
            //    JsonConvert.DeserializeObject<int[]>(json),
            //    inputColumnName, outputColumnName));
        }

        public void SaveToFile(string fileName)
        {
            //File.WriteAllText(fileName, JsonConvert.SerializeObject(_featureIndices, Formatting.Indented));
        }

        public DataViewSchema GetOutputSchema(DataViewSchema inputSchema)
        {
            var annotationBuilder = new DataViewSchema.Annotations.Builder();
            annotationBuilder.AddPrimitiveValue<bool>("IsNormalized", BooleanDataViewType.Instance, true);
            var schemaBuilder = new DataViewSchema.Builder();
            schemaBuilder.AddColumn(_inputColumnName, new VectorDataViewType((
                (VectorDataViewType)inputSchema[_inputColumnName].Type).ItemType, _pca.NumberOfOutputs),
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
            throw new NotImplementedException();
        }

        public IDataView Transform(IDataView input)
        {
            var features = input.GetColumn<float[]>(_inputColumnName)
                .Select(x => Array.ConvertAll(x, y => (double)y)).ToArray();
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();
            var values = input.GetColumn<float>("Value").ToArray();
            var dayOfTheWeek = input.GetColumn<float>("DayOfTheWeek").ToArray();
            var month = input.GetColumn<float>("Month").ToArray();

            var pcaTransformed = _pca.Transform(features);

            var output = new List<StockRiseDataFeature>();
            for (int i = 0; i < pcaTransformed.Length; ++i)
            {
                output.Add(new StockRiseDataFeature(Array.ConvertAll(pcaTransformed[i], y => (float)y).ToArray(),
                    labels[i], values[i], dayOfTheWeek[i], month[i]));
            }

            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, _pca.NumberOfOutputs);
            return _mLContext.Data.LoadFromEnumerable(output, definedSchema);
        }
    }
}
