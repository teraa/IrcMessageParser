using System;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests.Parsing;

public class TagsParserTests
{
    private readonly TagsParser _parser = new TagsParser();

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

    [Fact]
    public void Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

    [Fact]
    public void SingleTag()
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
    public void MultipleTags()
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
    public void EqualsSignInValue_TreatedAsValue()
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
    public void TagWithPrefixPlus()
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
    public void Flag_EmptyValue()
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
    public void FlagWithTrailing_EmptyValue()
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
    public void RepeatedKey_OverwritesValue()
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
    public void RepeatedFlagKey_OverwritesValue()
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

    [Theory]
    [InlineData("x", TagsParser.Result.Success)]
    [InlineData("", TagsParser.Result.Empty)]
    [InlineData(";", TagsParser.Result.TrailingSemicolon)]
    [InlineData("x;", TagsParser.Result.TrailingSemicolon)]
    [InlineData("x;;x", TagsParser.Result.KeyEmpty)]
    internal void ParseResultTest(string input, TagsParser.Result expectedResult)
    {
        TagsParser.Result result = TagsParser.Parse(input, out _);
        Assert.Equal(expectedResult, result);

        if (result is TagsParser.Result.Success)
        {
            _ = _parser.Parse(input);
        }
        else
        {
            Assert.Throws<FormatException>(
                () => _parser.Parse(input)
            );
        }
    }
}
