using Xunit;

namespace Teraa.Irc.Tests;

public class TagValueTests
{
    [Theory]
    // Normal
    [InlineData(@"", @"")]
    [InlineData(@"x", @"x")]
    // Special
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
    // Normal
    [InlineData(@"", @"")]
    [InlineData(@"x", @"x")]
    // Special
    [InlineData(@"\\", @"\")]
    [InlineData(@"\:", ";")]
    [InlineData(@"\s", " ")]
    [InlineData(@"\r", "\r")]
    [InlineData(@"\n", "\n")]
    [InlineData(@"\x", "x")]
    [InlineData(@"\0", "0")]
    [InlineData(@"\?", "?")]
    [InlineData(@"\\s", @"\s")]
    [InlineData(@"one\stwo\sthree", "one two three")]
    [InlineData(@"abc\", @"abc\")]
    [InlineData(@"abc\s", @"abc ")]
    [InlineData(@"abc\s1", @"abc 1")]
    [InlineData(@"abc\s12", @"abc 12")]
    [InlineData(@"abc\s123", @"abc 123")]
    [InlineData(@"ab\s", @"ab ")]
    [InlineData(@"a\s", @"a ")]
    [InlineData(@"plain", @"plain")]
    public void ParseValueTest(string input, string parsed)
    {
        var actualParsed = Tags.ParseValue(input);
        Assert.Equal(parsed, actualParsed);
    }
}
