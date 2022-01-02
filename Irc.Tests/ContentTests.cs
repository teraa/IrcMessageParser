using System;
using Xunit;
using static Teraa.Irc.Content;

namespace Teraa.Irc.Tests;

public class ContentTests
{
    [Fact]
    public void NoCtcp_Parse()
    {
        var raw = "text";
        var content = Content.Parse(raw);

        Assert.Equal(raw, content.Text);
        Assert.Null(content.Ctcp);
        Assert.Equal(raw, content);
    }

    [Fact]
    public void Ctcp_Parse()
    {
        var raw = "\u0001ACTION text\u0001";
        var content = Content.Parse(raw);

        Assert.Equal("text", content.Text);
        Assert.Equal("ACTION", content.Ctcp);
        Assert.Equal(raw, content);
    }

    [Fact]
    public void CtcpMissingEndDelimiter_Parse()
    {
        var raw = "\u0001ACTION text";
        var content = Content.Parse(raw);

        Assert.Equal("text", content.Text);
        Assert.Equal("ACTION", content.Ctcp);
        Assert.Equal(raw + "\u0001", content);
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
    [InlineData("x", ParseStatus.Success)]
    [InlineData("", ParseStatus.FailEmpty)]
    [InlineData("\u0001ACTION", ParseStatus.FailMissingCtcpEnding)]
    internal void ParseStatusTest(string input, ParseStatus expectedStatus)
    {
        var status = Content.Parse(input, out _);
        Assert.Equal(expectedStatus, status);

        var success = Content.TryParse(input, out _);
        Assert.Equal(status is ParseStatus.Success, success);

        if (status is ParseStatus.Success)
        {
            _ = Content.Parse(input);
        }
        else
        {
            Assert.Throws<FormatException>(
                () => Content.Parse(input)
            );
        }
    }
}