using System;
using System.Collections.Generic;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests;

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

    [Theory]
    [InlineData("x", TagsParser.Result.Success)]
    [InlineData("", TagsParser.Result.Empty)]
    [InlineData(";", TagsParser.Result.TrailingSemicolon)]
    [InlineData("x;", TagsParser.Result.TrailingSemicolon)]
    [InlineData("x;;x", TagsParser.Result.KeyEmpty)]
    internal void Parse_ResultTest(string input, TagsParser.Result expectedResult)
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

    [Fact]
    public void ToString_SingleTag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = "value",
        });

        Assert.Equal("key=value", _parser.ToString(tags));
    }

    [Fact]
    public void ToString_EmptyAsFlag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = "",
        });

        Assert.Equal("key", _parser.ToString(tags));
    }

    [Fact]
    public void ToString_NullAsFlag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = null!,
        });

        Assert.Equal("key", _parser.ToString(tags));
    }

    [Fact]
    public void ToString_PrefixedTag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["+key"] = "value",
        });

        Assert.Equal("+key=value", _parser.ToString(tags));
    }

    [Fact]
    public void ToString_MultipleTags()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["one"] = "[one]",
            ["two"] = "",
            ["three"] = "[three]",
            ["four"] = null!,
        });

        Assert.Equal("one=[one];two;three=[three];four", _parser.ToString(tags));
    }
}
