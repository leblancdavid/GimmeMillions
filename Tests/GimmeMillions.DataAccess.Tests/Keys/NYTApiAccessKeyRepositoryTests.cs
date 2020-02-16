using FluentAssertions;
using GimmeMillions.DataAccess.Keys;
using GimmeMillions.Domain.Keys;
using System.IO;
using System.Linq;
using Xunit;

namespace GimmeMillions.DataAccess.Tests.Keys
{
    public class NYTApiAccessKeyRepositoryTests
    {
        private readonly string _pathToKeys = "../../../../Repository/Keys";

        [Fact]
        public void ShouldAddNewKeys()
        {
            Directory.Exists(_pathToKeys).Should().BeTrue();
            var repo = new NYTApiAccessKeyRepository(_pathToKeys);

            var result = repo.AddOrUpdateKey(new AccessKey("RJMISprGhvdA1gf9sKqaar0B0VR4457I", "q6ty6EmesuBXNjzY", "active"));
            result.IsFailure.Should().BeFalse();
            File.Exists($"{_pathToKeys}/RJMISprGhvdA1gf9sKqaar0B0VR4457I.json").Should().BeTrue();
        }

        [Fact]
        public void ShouldGetKeys()
        {
            Directory.Exists(_pathToKeys).Should().BeTrue();
            var repo = new NYTApiAccessKeyRepository(_pathToKeys);

            var results = repo.GetKeys();
            results.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public void ShouldGetActiveKeys()
        {
            Directory.Exists(_pathToKeys).Should().BeTrue();
            var repo = new NYTApiAccessKeyRepository(_pathToKeys);

            var results = repo.GetActiveKeys();
            results.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public void ShouldGetSpecificKey()
        {
            Directory.Exists(_pathToKeys).Should().BeTrue();
            var repo = new NYTApiAccessKeyRepository(_pathToKeys);

            var result = repo.GetKey("RJMISprGhvdA1gf9sKqaar0B0VR4457I");
            result.IsSuccess.Should().BeTrue();
        }
    }
}
