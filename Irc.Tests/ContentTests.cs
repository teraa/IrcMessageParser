using System;
using Xunit;

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
    public void CtcpMissingEnding_ParseThrows()
    {
        Assert.Throws<FormatException>(
            () => Content.Parse("\u0001ACTION")
        );
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
}
