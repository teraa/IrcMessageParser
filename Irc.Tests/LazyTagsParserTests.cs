using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests;

public class LazyTagsParserTests
{
    private readonly LazyTagsParser _parser = new LazyTagsParser();

    [Theory]
    [InlineData("key=value", "key", "value")]
    [InlineData("key=", "key", "")]
    [InlineData("key", "key", "")]
    [InlineData("a;b=1;c=2", "b", "1")]
    public void Indexer_Tests(string input, string key, string expectedValue)
    {
        var tags = _parser.Parse(input);
        Assert.Equal(expectedValue, tags[key]);
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
