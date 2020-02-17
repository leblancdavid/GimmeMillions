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
        public void ShouldAddAllTheNewKeys()
        {
            var repo = new NYTApiAccessKeyRepository(_pathToKeys);

            repo.AddOrUpdateKey(new AccessKey("RJMISprGhvdA1gf9sKqaar0B0VR4457I", "q6ty6EmesuBXNjzY", "active"));
            repo.AddOrUpdateKey(new AccessKey("0538LTvIsMsrE0g0bOD4goNCqif4cc8F", "Hjz1TTHsbJorL5La", "active"));
            repo.AddOrUpdateKey(new AccessKey("PZ0ZGSbJehjgLsAi04J0CsL8WNOhSDG4", "3lB9snAEj1qt3CvW", "active"));
            repo.AddOrUpdateKey(new AccessKey("Csq0qi1m43msta0ASYwlbbLmJHN4XRXt", "oU2IUU9FAtYtqq5v", "active"));
            repo.AddOrUpdateKey(new AccessKey("PDAtGMIZDflk89b2IF06KnzVfbDI04em", "To8qImFmuq8fg6Vn", "active"));
            repo.AddOrUpdateKey(new AccessKey("TEMQL0veAU5XVUAVU8q2YU06IlUcaQ50", "hXp70BYuJIVodWrU", "active"));
            repo.AddOrUpdateKey(new AccessKey("WSDoVCZZrWi8nmQEqV0HKUpms26tI6O4", "IF51Hq58yd0wYgRo", "active"));
            repo.AddOrUpdateKey(new AccessKey("aE9y0JWiw3AP4wC0z3W9iLPBprtpY6e9", "LZL7krIhxqt2dhLk", "active"));
            repo.AddOrUpdateKey(new AccessKey("uf00cfbmQCG9FvEXQck0zr0f7EI5Paam", "KNduFFrIc02HzOVU", "active"));
            repo.AddOrUpdateKey(new AccessKey("1rLdtJFH7uEnLyen3ko1HCCAzP2xG36Z", "KNduFFrIc02HzOVU", "active"));
            repo.AddOrUpdateKey(new AccessKey("lC6rUHjXO8HZp3S1ial0kQQnFEOxHMIz", "90dpdw8KdT0mBD8T", "active"));
            repo.AddOrUpdateKey(new AccessKey("uq43KaTozKJ3mvQwEFMVF36kdFZPDFqt", "GWpdWhOHUM2h0KBp", "active"));
            repo.AddOrUpdateKey(new AccessKey("0df9aGwP0rwRy1oPNp6eHQk6Yi4kmlsm", "JDlNkBUox0JJBhKg", "active"));
            repo.AddOrUpdateKey(new AccessKey("IkOymJo2Gqk0idkLmP0jfYs2X3sXZZ0k", "Qbj0Ah7j59ip1C8b", "active"));
            repo.AddOrUpdateKey(new AccessKey("OaLjqH6bay4bUhv3xY40hXbznePIPeEv", "IdcjZ42umnNyeN8W", "active"));
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
