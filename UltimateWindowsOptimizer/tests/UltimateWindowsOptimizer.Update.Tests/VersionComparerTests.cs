using UltimateWindowsOptimizer.Update.Services;
using Xunit;

namespace UltimateWindowsOptimizer.Update.Tests;

public class VersionComparerTests
{
    [Theory]
    [InlineData("1.10.0", "1.9.0", 1)]
    [InlineData("1.9.0", "1.10.0", -1)]
    [InlineData("1.2.0", "1.2.0", 0)]
    [InlineData("2.0.0", "1.9.9", 1)]
    [InlineData("1.0.1", "1.0.0", 1)]
    [InlineData("1.0", "1.0.0", 0)]
    [InlineData("10.0.0", "9.99.99", 1)]
    public void Compare_SemanticOrder(string a, string b, int expectedSign)
    {
        var result = VersionComparer.Compare(a, b);
        Assert.Equal(Math.Sign(expectedSign), Math.Sign(result));
    }

    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData("0.0.1", true)]
    [InlineData("", false)]
    [InlineData("abc", false)]
    [InlineData("1.2.3.4.5", false)]
    public void IsValid(string version, bool expected)
    {
        Assert.Equal(expected, VersionComparer.IsValid(version));
    }

    [Fact]
    public void IsNewer_Works()
    {
        Assert.True(VersionComparer.IsNewer("1.3.0", "1.2.0"));
        Assert.False(VersionComparer.IsNewer("1.2.0", "1.2.0"));
        Assert.False(VersionComparer.IsNewer("1.1.0", "1.2.0"));
    }
}
