using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GimmeMillions.Domain.Features
{
    public class AKMBoWFeatureVectorExtractor : IFeatureExtractor<Article>
    {
        public string Encoding { get; set; }
        private int _rank;
        private MLContext _mLContext;
        private DataViewSchema _dataSchema;
        private ITransformer _transformer;
        private IFeatureExtractor<Article> _featureExtractor;
        private int Version = 1;


        public AKMBoWFeatureVectorExtractor(IFeatureExtractor<Article> featureExtractor, int rank)
        {
            _mLContext = new MLContext();
            _featureExtractor = featureExtractor;
            _rank = rank;
            Encoding = $"AKMBoW{_rank}v{Version}";
        }

        public double[] Extract(IEnumerable<(Article Data, float Weight)> data)
        {
            var articlesFeature = _featureExtractor.Extract(data);
            //Load the data into a view
            var inputDataView = _mLContext.Data.LoadFromEnumerable(
                new List<ArticleFeatures>()
                {
                    new ArticleFeatures(Array.ConvertAll(articlesFeature, y => (float)y))
                },
                GetSchemaDefinition(articlesFeature.Length));

            var transformedData = _transformer.Transform(inputDataView);

            return Array.ConvertAll(transformedData.GetColumn<float[]>("Features").First().ToArray(), x => (double)x);
        }

        public Result Load(string pathToModel)
        {
            try
            {
                string directory = $"{pathToModel}/{Encoding}";

                DataViewSchema schema = null;
                _transformer = _mLContext.Model.Load($"{directory}/{Encoding}_Model.zip", out schema);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to load the model: {ex.Message}");
            }
        }

        public Result Save(string pathToModel)
        {
            try
            {
                string directory = $"{pathToModel}/{Encoding}";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _mLContext.Model.Save(_transformer, _dataSchema, $"{directory}/{Encoding}_Model.zip");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to save the model: {ex.Message}");
            }
        }

        public void Train(IEnumerable<FeatureVector> articleFeatures)
        {
            
            var datasetView = _mLContext.Data.LoadFromEnumerable(
                articleFeatures.Select(x =>
                {
                    return new ArticleFeatures(Array.ConvertAll(x.Data, y => (float)y));
                }),
                GetSchemaDefinition(articleFeatures.FirstOrDefault().Length));

            _dataSchema = datasetView.Schema;
            _transformer = _mLContext.Transforms.ApproximatedKernelMap("Features", rank: _rank).Fit(datasetView);
        }

        private SchemaDefinition GetSchemaDefinition(int length)
        {
            var definedSchema = SchemaDefinition.Create(typeof(ArticleFeatures));
            var featureColumn = definedSchema["Features"].ColumnType as VectorDataViewType;
            var vectorItemType = ((VectorDataViewType)definedSchema[0].ColumnType).ItemType;
            definedSchema[0].ColumnType = new VectorDataViewType(vectorItemType, length);

            return definedSchema;
        }

        public class ArticleFeatures
        {
            public float[] Features { get; set; }

            public ArticleFeatures(float[] features)
            {
                Features = features;
            }
        }
    }
}
