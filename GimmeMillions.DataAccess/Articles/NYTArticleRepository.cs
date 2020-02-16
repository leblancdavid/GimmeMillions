using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.DataAccess.Articles
{
    public class NYTArticleRepository : IArticleRepository
    {
        private string _pathToKeys;
        public NYTArticleRepository(string pathToKeys)
        {
            _pathToKeys = pathToKeys;
        }

        public Result AddOrUpdate(Article article)
        {
            try
            {
                string directory =  $"{_pathToKeys}/{article.Date.ToString("yyyy/MM/dd")}";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string fileName = $"{directory}/{article.Uri}.json";
                File.WriteAllText(fileName, JsonConvert.SerializeObject(article, Formatting.Indented));
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error occurred while adding or updating an article: {ex.Message}");
            }
        }

        public bool ContainsArticles(DateTime dateTime)
        {
            string directory = $"{_pathToKeys}/{dateTime.ToString("yyyy/MM/dd")}";
            if (!Directory.Exists(directory))
            {
                return false;
            }

            var files = Directory.GetFiles(directory);
            if(files.Length == 0)
            {
                return false;
            }

            return true;
        }

        public IEnumerable<Article> GetArticles(DateTime dateTime)
        {
            var articles = new List<Article>();

            string directory = $"{_pathToKeys}/{dateTime.ToString("yyyy/MM/dd")}";
            if (!Directory.Exists(directory))
            {
                return articles;
            }

            var files = Directory.GetFiles(directory);
            foreach(var file in files)
            {
                var jsonKey = File.ReadAllText(file);
                var key = JsonConvert.DeserializeObject<Article>(jsonKey);
                articles.Add(key);
            }

            return articles;
        }
    }
}
