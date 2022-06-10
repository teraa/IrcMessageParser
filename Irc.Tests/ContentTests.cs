using System;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests;

public class ContentTests
{
    private readonly IContentParser _parser = new ContentParser();

    [Fact]
    public void Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

    [Fact]
    public void NoCtcp_Parse()
    {
        var raw = "text";
        var content = _parser.Parse(raw);

        Assert.Equal(raw, content.Text);
        Assert.Null(content.Ctcp);
        Assert.Equal(raw, content.ToString());
    }

    [Fact]
    public void Ctcp_Parse()
    {
        var raw = "\u0001ACTION text\u0001";
        var content = _parser.Parse(raw);

        Assert.Equal("text", content.Text);
        Assert.Equal("ACTION", content.Ctcp);
        Assert.Equal(raw, content.ToString());
    }

    [Fact]
    public void CtcpMissingEndDelimiter_Parse()
    {
        var raw = "\u0001ACTION text";
        var content = _parser.Parse(raw);

        Assert.Equal("text", content.Text);
        Assert.Equal("ACTION", content.Ctcp);
        Assert.Equal(raw + "\u0001", content.ToString());
    }

    [Fact]
    public void NoCtcp_ToString()
    {
        var content = new Content("text");
        Assert.Equal("text", content.ToString());
    }

    [Fact]
    public void Ctcp_ToString()
    {
        var content = new Content("text", "ACTION");
        Assert.Equal("\u0001ACTION text\u0001", content.ToString());
    }

    [Theory]
    [InlineData("x", ContentParser.Result.Success)]
    [InlineData("", ContentParser.Result.Empty)]
    [InlineData("\u0001ACTION", ContentParser.Result.MissingCtcpEnding)]
    internal void ParseResultTest(string input, ContentParser.Result expectedResult)
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
}
