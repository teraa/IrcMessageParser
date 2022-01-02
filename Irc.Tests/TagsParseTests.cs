using System;
using Xunit;
using static Teraa.Irc.Tags;

namespace Teraa.Irc.Tests;

public class TagsParseTests
{
    [Fact]
    public void MultipleTags()
    {
        var tags = Tags.Parse("A;B=;C=c\\;D=d");

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
        var tags = Tags.Parse("key=value=value");

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
        var tags = Tags.Parse("+key=value");

        Assert.Collection(tags,
            pair =>
            {
                Assert.Equal("+key", pair.Key);
                Assert.Equal("value", pair.Value);
            }
        );
    }

    [Fact]
    public void TrailingSemicolon_Ignored()
    {
        var tags = Tags.Parse("key=value");

        Assert.Collection(tags,
            pair =>
            {
                Assert.Equal("key", pair.Key);
                Assert.Equal("value", pair.Value);
            }
        );
    }

    [Fact]
    public void Flag_EmptyValue()
    {
        var tags = Tags.Parse("A");

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
        var tags = Tags.Parse("A=");

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
        var tags = Tags.Parse("A=a1;B=b;A=a2");

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
        var tags = Tags.Parse("A=a;A");

        Assert.Collection(tags,
            tag =>
            {
                Assert.Equal("A", tag.Key);
                Assert.Equal("", tag.Value);
            }
        );
    }

    [Theory]
    [InlineData("x", ParseStatus.Success)]
    [InlineData("", ParseStatus.FailEmpty)]
    [InlineData(";", ParseStatus.FailTrailingSemicolon)]
    [InlineData("x;", ParseStatus.FailTrailingSemicolon)]
    [InlineData("x;;x", ParseStatus.FailKeyEmpty)]
    internal void ParseStatusTest(string input, ParseStatus expectedStatus)
    {
        var status = Tags.Parse(input, out _);
        Assert.Equal(expectedStatus, status);

        var success = Tags.TryParse(input, out _);
        Assert.Equal(status is ParseStatus.Success, success);

        if (status is ParseStatus.Success)
        {
            _ = Tags.Parse(input);
        }
        else
        {
            Assert.Throws<FormatException>(
                () => Tags.Parse(input)
            );
        }
    }
}
