using FluentAssertions;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Tests.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.DataAccess.Tests.Features
{
    public class FeatureDictionaryJsonRepositoryTests
    {
        private readonly string _pathToArticles = "../../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../../Repository/Languages";
        [Fact]
        public void ShouldAddFeatureDictionaries()
        {
            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.AddOrUpdate(FeaturesDictionaryTests.CreateTestFeatureDictionary(
                "FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries",
                _pathToArticles, _pathToLanguage, new DateTime(2000, 1, 1), new DateTime(2000, 1, 2)));

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ShouldAddFeatureDictionary_USA()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa.txt"));

            int index = 0;
            var featureDictionary = new FeaturesDictionary();
            featureDictionary.DictionaryId = "USA";
            foreach (var word in featureChecker.LanguageSet)
            {
                featureDictionary[word] = index;
                index++;
            }

            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.AddOrUpdate(featureDictionary);
            

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ShouldAddFeatureDictionary_USA_NoShortWords()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/usa-no-small-words.txt"));

            int index = 0;
            var featureDictionary = new FeaturesDictionary();
            featureDictionary.DictionaryId = "USA-f2";
            foreach (var word in featureChecker.LanguageSet)
            {
                featureDictionary[word] = index;
                index++;
            }

            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.AddOrUpdate(featureDictionary);


            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ShouldAddFeatureDictionary_Google_medium_and_large_words()
        {
            var featureChecker = new UsaLanguageChecker();
            featureChecker.Load(new StreamReader($"{_pathToLanguage}/google-10000-english-usa-no-swears-medium+long.txt"));

            int index = 0;
            var featureDictionary = new FeaturesDictionary();
            featureDictionary.DictionaryId = "Google-M+L";
            foreach (var word in featureChecker.LanguageSet)
            {
                featureDictionary[word] = index;
                index++;
            }

            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.AddOrUpdate(featureDictionary);


            result.IsSuccess.Should().BeTrue();
        }


        [Fact]
        public void ShouldGetFeatureDictionary()
        {
            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.GetFeatureDictionary("FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries");

            result.IsSuccess.Should().BeTrue();
            result.Value.Size.Should().Be(31213);
        }

        [Fact]
        public void ShouldGetTheSetOfAvailableFeatureDictionaries()
        {
            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.GetFeatureDictionaryIds();

            result.Count().Should().BeGreaterThan(0);
        }
    }
}
