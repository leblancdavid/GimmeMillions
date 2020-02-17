﻿using GimmeMillions.DataAccess.Articles;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.Domain.Articles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NytArticleCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            var lastCollectedDate = new DateTime(2000, 1, 1);
            string progressFile = "progress.json";
            if(File.Exists(progressFile))
            {
                var jsonDate = File.ReadAllText(progressFile);
                lastCollectedDate = JsonConvert.DeserializeObject<DateTime>(jsonDate);
            }

            var keysRepo = new NYTApiAccessKeyRepository("../../../../Repository/Keys");
            var articlesRepo = new NYTArticleRepository("../../../../Repository/Articles");

            var accessService = new NYTArticleAccessService(keysRepo, articlesRepo);

            int retryCount = 0;
            while (lastCollectedDate != DateTime.Today && retryCount < 5)
            {
                File.WriteAllText(progressFile, JsonConvert.SerializeObject(lastCollectedDate, Formatting.Indented));
                Console.WriteLine($"Retrieving articles from {lastCollectedDate.ToString("yyyy/MM/dd")}...");
                var articles = accessService.GetArticles(lastCollectedDate, new List<FilterQuery>());
                if(articles.IsFailure)
                {
                    Console.WriteLine($"Could not retrieve articles: '{articles.Error}'");
                    retryCount++;
                }
                else
                {
                    Console.WriteLine($"Found {articles.Value.Count()} articles for {lastCollectedDate.ToString("yyyy/MM/dd")}");
                    lastCollectedDate.AddDays(1.0);
                    retryCount = 0;
                }

            }
        }
    }
}
