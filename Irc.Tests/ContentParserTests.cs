using System;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests;

public class ContentParserTests
{
    private readonly IContentParser _parser = new ContentParser();

    [Fact]
    public void Parse_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

    [Fact]
    public void Parse_NoCtcp()
    {
        var raw = "text";
        var content = _parser.Parse(raw);

        Assert.Equal(raw, content.Text);
        Assert.Null(content.Ctcp);
        Assert.Equal(raw, _parser.ToString(content));
    }

    [Fact]
    public void Parse_Ctcp()
    {
        var raw = "\u0001ACTION text\u0001";
        var content = _parser.Parse(raw);

        Assert.Equal("text", content.Text);
        Assert.Equal("ACTION", content.Ctcp);
        Assert.Equal(raw, _parser.ToString(content));
    }

    [Fact]
    public void Parse_CtcpMissingEndDelimiter()
    {
        var raw = "\u0001ACTION text";
        var content = _parser.Parse(raw);

        Assert.Equal("text", content.Text);
        Assert.Equal("ACTION", content.Ctcp);
        Assert.Equal(raw + "\u0001", _parser.ToString(content));
    }

    [Theory]
    [InlineData("x", ContentParser.Result.Success)]
    [InlineData("", ContentParser.Result.Empty)]
    [InlineData("\u0001ACTION", ContentParser.Result.MissingCtcpEnding)]
    internal void Parse_ResultTest(string input, ContentParser.Result expectedResult)
    {
        ContentParser.Result result = ContentParser.Parse(input, out _);
        Assert.Equal(expectedResult, result);

        if (result is ContentParser.Result.Success)
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
    public void ToString_NoCtcp()
    {
        var content = new Content("text");
        Assert.Equal("text", _parser.ToString(content));
    }

    [Fact]
    public void ToString_Ctcp()
    {
        var content = new Content("text", "ACTION");
        Assert.Equal("\u0001ACTION text\u0001", _parser.ToString(content));
    }
}
