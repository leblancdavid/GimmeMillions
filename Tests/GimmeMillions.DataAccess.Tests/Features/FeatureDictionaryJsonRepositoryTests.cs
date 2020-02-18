using FluentAssertions;
using GimmeMillions.DataAccess.Features;
using GimmeMillions.Domain.Tests.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GimmeMillions.DataAccess.Tests.Features
{
    public class FeatureDictionaryJsonRepositoryTests
    {
        private readonly string _pathToArticles = "../../../../Repository/Articles";
        private readonly string _pathToDictionary = "../../../../Repository/Dictionaries";
        private readonly string _pathToLanguage = "../../../../Repository/Languages";
        [Fact]
        public void ShouldAddFeatureDictionaries()
        {
            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.AddOrUpdate(FeaturesDictionaryTests.CreateTestFeatureDictionary(
                "FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries",
                _pathToArticles, _pathToLanguage));

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ShouldGetFeatureDictionary()
        {
            var repo = new FeatureDictionaryJsonRepository(_pathToDictionary);
            var result = repo.GetFeatureDictionary("FeatureDictionaryJsonRepositoryTests.ShouldAddFeatureDictionaries");

            result.IsSuccess.Should().BeTrue();
            result.Value.MaxCount.Should().Be(112378);
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
