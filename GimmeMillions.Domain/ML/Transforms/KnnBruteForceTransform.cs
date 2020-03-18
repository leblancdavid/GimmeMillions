using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.ML.Transforms
{
    public class KnnBruteForceTransform : ITransformer
    {
        private MLContext _mLContext;
        private (float[] Input, bool Label, float Value)[] _features;
        public bool IsRowToRowMapper => true;

        public KnnBruteForceTransform(MLContext mLContext,
            IDataView trainingData)
        {
            _mLContext = mLContext;

            var features = trainingData.GetColumn<float[]>("Features").ToArray();
            var labels = trainingData.GetColumn<bool>("Label").ToArray();
            var values = trainingData.GetColumn<float>("Value").ToArray();

            _features = new (float[] Input, bool Label, float Value)[features.Length];
            for (int i = 0; i < features.Length; ++i)
            {
                _features[i] = (features[i], labels[i], values[i]);
            }
        }

        public DataViewSchema GetOutputSchema(DataViewSchema inputSchema)
        {
            var schemaBuilder = new DataViewSchema.Builder();
            schemaBuilder.AddColumn("Score", inputSchema["Value"].Type);
            schemaBuilder.AddColumn("PredictedLabel", inputSchema["Label"].Type);
            schemaBuilder.AddColumn("Probabily", inputSchema["Value"].Type);
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
            var inputFeatures = input.GetColumn<float[]>("Features");
            var predictions = new List<KnnPrediction>();
            foreach(var vector in inputFeatures)
            {
                predictions.Add(GetPrediction(vector, 10));
            }
            var definedSchema = SchemaDefinition.Create(typeof(KnnPrediction));
            return _mLContext.Data.LoadFromEnumerable(predictions, definedSchema);
        }

        private class KnnPrediction
        {
            public float Score;
            public float Probability;
            public bool PredictedLabel;
        }
        private KnnPrediction GetPrediction(float[] input, int nearestNeigbors)
        {
            var distances = ComputeDistances(input).Take(nearestNeigbors);
            var positiveSum = distances.Sum(x => x.Label ? x.Distance : 0.0);
            var negativeSum = distances.Sum(x => !x.Label ? x.Distance : 0.0);

            double p = negativeSum / (negativeSum + positiveSum);
            return new KnnPrediction()
            {
                Probability = (float)p,
                PredictedLabel = p > 0.5f,
                Score = (float)(positiveSum - negativeSum)
            };
        }

        private (double Distance, bool Label, float Value)[] ComputeDistances(float[] input)
        {
            var distances = new (double Distance, bool Label, float Value)[_features.Length];
            for(int i = 0; i < _features.Length; ++i)
            {
                distances[i] = (Distance(input, _features[i].Input), _features[i].Label, _features[i].Value);
            }

            return distances.OrderBy(x => x.Distance).ToArray();
        }

        private double Distance(float[] v1, float[] v2)
        {
            double distance = 0.0f;
            for(int i = 0; i < v1.Length; ++i)
            {
                distance += Math.Pow(v1[i] - v2[i], 2);
            }

            return Math.Sqrt(distance);
        }
    }
}
