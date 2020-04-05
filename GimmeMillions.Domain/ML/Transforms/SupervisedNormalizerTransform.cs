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
        private (double pMean, double pStdev, double nMean, double nStdev)[] _statistics;
        private string _inputColumnName;
        private string _outputColumnName;
        private MLContext _mLContext;

        public bool IsRowToRowMapper => true;

        public SupervisedNormalizerTransform(MLContext mLContext,
            (double pMean, double pStdev, double nMean, double nStdev)[] statistics,
            string inputColumnName = "News",
            string outputColumnName = "Label")
        {
            _mLContext = mLContext;
            _inputColumnName = inputColumnName;
            _outputColumnName = outputColumnName;
            _statistics = statistics;
        }

        public static Result<SupervisedNormalizerTransform> LoadFromFile(string fileName,
            MLContext mLContext,
            string inputColumnName = "News",
            string outputColumnName = "Label")
        {
            if (!File.Exists(fileName))
            {
                return Result.Failure<SupervisedNormalizerTransform>($"SupervisedNormalizerTransform model named {fileName} could not be found");
            }
            var json = File.ReadAllText(fileName);
            return Result.Ok(new SupervisedNormalizerTransform(mLContext,
                JsonConvert.DeserializeObject<(double pMean, double pStdev, double nMean, double nStdev)[]>(json),
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
            var features = input.GetColumn<double[]>(_inputColumnName).ToArray();
            var candlesticks = input.GetColumn<double[]>("Candlestick").ToArray();
            var labels = input.GetColumn<bool>(_outputColumnName).ToArray();
            var values = input.GetColumn<double>("Value").ToArray();
            var dayOfTheWeek = input.GetColumn<double>("DayOfTheWeek").ToArray();
            var month = input.GetColumn<double>("Month").ToArray();

            var output = new List<StockRiseDataFeature>();
            for (int i = 0; i < features.Length; ++i)
            {
                var normalized = new double[_statistics.Length];
                for (int j = 0; j < _statistics.Length; ++j)
                {
                    double pos = (features[i][j] - _statistics[j].pMean) / _statistics[j].pStdev;
                    double neg = (features[i][j] - _statistics[j].nMean) / _statistics[j].nStdev;
                    if(double.IsNaN(pos))
                    {
                        pos = 0.0f;
                    }
                    if(double.IsNaN(neg))
                    {
                        neg = 0.0f;
                    }

                    //float pF = 1.0f, nF = 1.0f;
                    //if (_statistics[j].pMean < _statistics[j].nMean)
                    //{
                    //    normalized[j] = pos;
                    //}
                    //else
                    //{
                    //    normalized[j] = neg;
                    //}

                    normalized[j] = Math.Abs(neg) - Math.Abs(pos);
                }
                output.Add(new StockRiseDataFeature(normalized, candlesticks[i], labels[i], values[i], dayOfTheWeek[i], month[i]));
            }

            int featureDimension = _statistics.Length;
            var definedSchema = SchemaDefinition.Create(typeof(StockRiseDataFeature));
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, featureDimension);
            return _mLContext.Data.LoadFromEnumerable(output, definedSchema);
        }
    }
}
