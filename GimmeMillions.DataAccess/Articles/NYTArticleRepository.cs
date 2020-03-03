﻿using CSharpFunctionalExtensions;
using GimmeMillions.Domain.Articles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GimmeMillions.DataAccess.Articles
{
    public class NYTArticleRepository : IArticleRepository
    {
        private string _pathToArticles;
        public NYTArticleRepository(string pathToArticles)
        {
            _pathToArticles = pathToArticles;
        }

        public Result AddOrUpdate(Article article)
        {
            try
            {
                string directory =  $"{_pathToArticles}/{article.Date.ToString("yyyy/MM/dd")}";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string uniqueId = article.Uri.Split('/').Last();
                string fileName = $"{directory}/{uniqueId}.json";
                File.WriteAllText(fileName, JsonConvert.SerializeObject(article, Formatting.Indented));
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error occurred while adding or updating an article: {ex.Message}");
            }
        }

        public bool ArticleExists(string id)
        {
            return GetArticle(id).IsSuccess;
        }

        public bool ContainsArticles(DateTime dateTime)
        {
            string directory = $"{_pathToArticles}/{dateTime.ToString("yyyy/MM/dd")}";
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

        public Result<Article> GetArticle(string id)
        {
            var yearDirectories = Directory.GetDirectories(_pathToArticles);
            foreach(var year in yearDirectories)
            {
                var monthDirectories = Directory.GetDirectories($"{year}");
                foreach(var month in monthDirectories)
                {
                    var dayDirectories = Directory.GetDirectories($"{month}");
                    foreach(var day in dayDirectories)
                    {
                        var articleFiles = Directory.GetFiles($"{day}");
                        foreach(var articleFile in articleFiles)
                        {
                            var articleId = Path.GetFileNameWithoutExtension(articleFile);
                            if(articleId == id)
                            {
                                var jsonArticle = File.ReadAllText(articleFile);
                                var article = JsonConvert.DeserializeObject<Article>(jsonArticle);
                                if (article == null)
                                {
                                    continue;
                                }
                                return Result.Ok(article);
                            }
                        }
                    }
                }
            }

            return Result.Failure<Article>($"Unable to find article with ID '{id}'");
        }

        public IEnumerable<Article> GetArticles(DateTime dateTime)
        {
            var articles = new List<Article>();

            string directory = $"{_pathToArticles}/{dateTime.ToString("yyyy/MM/dd")}";
            if (!Directory.Exists(directory))
            {
                return articles;
            }

            var files = Directory.GetFiles(directory);
            foreach(var file in files)
            {
                var jsonArticle = File.ReadAllText(file);
                var article = JsonConvert.DeserializeObject<Article>(jsonArticle);
                if(article == null)
                {
                    continue;
                }
                articles.Add(article);
            }

            return articles;
        }

        public IEnumerable<Article> GetArticles()
        {
            var articles = new List<Article>();
            var yearDirectories = Directory.GetDirectories(_pathToArticles);
            foreach (var year in yearDirectories)
            {
                var monthDirectories = Directory.GetDirectories($"{year}");
                foreach (var month in monthDirectories)
                {
                    var dayDirectories = Directory.GetDirectories($"{month}");
                    foreach (var day in dayDirectories)
                    {
                        var articleFiles = Directory.GetFiles($"{day}");
                        foreach (var articleFile in articleFiles)
                        {
                            var jsonArticle = File.ReadAllText(articleFile);
                            var article = JsonConvert.DeserializeObject<Article>(jsonArticle);
                            if (article == null)
                            {
                                continue;
                            }
                            articles.Add(article);
                        }
                    }
                }
            }

            return articles;
        }
    }
}
