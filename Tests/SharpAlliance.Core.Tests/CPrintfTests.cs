using Xunit;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Tests;

public class CPrintfTests
{
    [Theory]
    [InlineData("%s %d", "Day 12")]
    [InlineData("%d / %d", "3 / 7")]
    public void FormatsCSpecifiers(string format, string expected)
    {
        var args = format.StartsWith("%s") ? new object[] { "Day", 12 } : new object[] { 3, 7 };
        Assert.Equal(expected, wprintf(format, args));
    }

    [Fact]
    public void WidthPrecisionAndFlags()
    {
        Assert.Equal("05|  ab|3.14|ff|100%", wprintf("%02d|%4s|%.2f|%x|%d%%", 5, "ab", 3.14159, 255, 100));
    }

    [Fact]
    public void DotNetFormatsStillWork()
    {
        Assert.Equal("a-b", wprintf("{0}-{1}", "a", "b"));
    }
}
