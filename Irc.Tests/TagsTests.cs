using System.Collections.Generic;
using Xunit;

namespace Teraa.Irc.Tests;

public class TagsTests
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

    [Fact]
    public void SingleTag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = "value",
        });

        Assert.Equal("key=value", tags.ToString());
    }

    [Fact]
    public void EmptyAsFlag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = "",
        });

        Assert.Equal("key", tags.ToString());
    }

    [Fact]
    public void NullAsFlag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = null!,
        });

        Assert.Equal("key", tags.ToString());
    }

    [Fact]
    public void PrefixedTag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["+key"] = "value",
        });

        Assert.Equal("+key=value", tags.ToString());
    }

    [Fact]
    public void MultipleTags()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["one"] = "[one]",
            ["two"] = "",
            ["three"] = "[three]",
            ["four"] = null!,
        });

        Assert.Equal("one=[one];two;three=[three];four", tags.ToString());
    }
}
