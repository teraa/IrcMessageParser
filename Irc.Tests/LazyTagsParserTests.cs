using System;
using System.Collections.Generic;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests;

public class LazyTagsParserTests
{
    private readonly LazyTagsParser _parser = new LazyTagsParser();

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
        var actualParsed = TagsParser.ParseValue(input);
        Assert.Equal(parsed, actualParsed);
    }

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
        var actualEscaped = TagsParser.EscapeValue(input);
        Assert.Equal(escaped, actualEscaped);
    }

    [Fact]
    public void Parse_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

    [Fact]
    public void Parse_SingleTag()
    {
        var tags = _parser.Parse("key=value");

        Assert.Collection(tags,
            pair =>
            {
                Assert.Equal("key", pair.Key);
                Assert.Equal("value", pair.Value);
            }
        );
    }

    [Fact]
    public void Parse_MultipleTags()
    {
        var tags = _parser.Parse("A;B=;C=c\\;D=d");

        Assert.Collection(tags,
            tag =>
            {
                Assert.Equal("A", tag.Key);
                Assert.Equal("", tag.Value);
            },
            tag =>
            {
                Assert.Equal("B", tag.Key);
                Assert.Equal("", tag.Value);
            },
            tag =>
            {
                Assert.Equal("C", tag.Key);
                Assert.Equal("c\\", tag.Value);
            },
            tag =>
            {
                Assert.Equal("D", tag.Key);
                Assert.Equal("d", tag.Value);
            }
        );
    }

    [Fact]
    public void Parse_EqualsSignInValue_TreatedAsValue()
    {
        var tags = _parser.Parse("key=value=value");

        Assert.Collection(tags,
            pair =>
            {
                Assert.Equal("key", pair.Key);
                Assert.Equal("value=value", pair.Value);
            }
        );
    }

    [Fact]
    public void Parse_TagWithPrefixPlus()
    {
        var tags = _parser.Parse("+key=value");

        Assert.Collection(tags,
            pair =>
            {
                Assert.Equal("+key", pair.Key);
                Assert.Equal("value", pair.Value);
            }
        );
    }

    [Fact]
    public void Parse_Flag_EmptyValue()
    {
        var tags = _parser.Parse("A");

        Assert.Collection(tags,
            pair =>
            {
                Assert.Equal("A", pair.Key);
                Assert.Equal("", pair.Value);
            }
        );
    }

    [Fact]
    public void Parse_FlagWithTrailing_EmptyValue()
    {
        var tags = _parser.Parse("A=");

        Assert.Collection(tags,
            pair =>
            {
                Assert.Equal("A", pair.Key);
                Assert.Equal("", pair.Value);
            }
        );
    }

    [Fact]
    public void Parse_RepeatedKey_OverwritesValue()
    {
        var tags = _parser.Parse("A=a1;B=b;A=a2");

        Assert.Collection(tags,
            tag =>
            {
                Assert.Equal("A", tag.Key);
                Assert.Equal("a2", tag.Value);
            },
            tag =>
            {
                Assert.Equal("B", tag.Key);
                Assert.Equal("b", tag.Value);
            }
        );
    }

    [Fact]
    public void Parse_RepeatedFlagKey_OverwritesValue()
    {
        var tags = _parser.Parse("A=a;A");

        Assert.Collection(tags,
            tag =>
            {
                Assert.Equal("A", tag.Key);
                Assert.Equal("", tag.Value);
            }
        );
    }

    [Fact]
    public void ToString_Literal()
    {
        var tags = new LazyTags("key=value;key=value");
        Assert.Equal("key=value;key=value", _parser.ToString(tags));
    }

    [Fact]
    public void ToString_Empty()
    {
        var tags = new LazyTags("");
        Assert.Equal("", _parser.ToString(tags));
    }
}
