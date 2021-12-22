using Xunit;

namespace Teraa.IrcMessageParser.Tests;

public class TagValueTests
{
    [Theory]
    [InlineData(@"\", @"\\")]
    [InlineData(";", @"\:")]
    [InlineData(" ", @"\s")]
    [InlineData("\r", @"\r")]
    [InlineData("\n", @"\n")]
    [InlineData(@"\\ ", @"\\\\\s")]
    public void EscapeValueTest(string input, string escaped)
    {
        var actualEscaped = Tags.EscapeValue(input);
        Assert.Equal(escaped, actualEscaped);
    }

    [Theory]
    [InlineData(@"\\", @"\")]
    [InlineData(@"\:", ";")]
    [InlineData(@"\s", " ")]
    [InlineData(@"\r", "\r")]
    [InlineData(@"\n", "\n")]
    [InlineData(@"\x", "x")]
    [InlineData(@"\0", "0")]
    [InlineData(@"\?", "?")]
    [InlineData(@"\\s", @"\s")]
    public void ParseValueTest(string input, string parsed)
    {
        var actualParsed = Tags.ParseValue(input);
        Assert.Equal(parsed, actualParsed);
    }
}
