using System;
using Xunit;

namespace Teraa.Irc.Tests;

public class PrefixTests
{
    [Fact]
    public void Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => Prefix.Parse(null!));
    }

    [Fact]
    public void NameOnly_Parse()
    {
        var raw = "servername";
        var prefix = Prefix.Parse(raw);

        Assert.Equal(raw, prefix.Name);
        Assert.Null(prefix.User);
        Assert.Null(prefix.Host);
    }

    [Fact]
    public void NickAndUser_Parse()
    {
        var raw = "nick!user";
        var prefix = Prefix.Parse(raw);

        Assert.Equal("nick", prefix.Name);
        Assert.Equal("user", prefix.User);
        Assert.Null(prefix.Host);
    }

    [Fact]
    public void NickAndHost_Parse()
    {
        var raw = "nick@host";
        var prefix = Prefix.Parse(raw);

        Assert.Equal("nick", prefix.Name);
        Assert.Null(prefix.User);
        Assert.Equal("host", prefix.Host);
    }

    [Fact]
    public void Full_Parse()
    {
        var raw = "nick!user@host";
        var prefix = Prefix.Parse(raw);

        Assert.Equal("nick", prefix.Name);
        Assert.Equal("user", prefix.User);
        Assert.Equal("host", prefix.Host);
    }

    [Fact]
    public void NameOnly_ToString()
    {
        var prefix = new Prefix(name: "servername", user: null, host: null);
        Assert.Equal("servername", prefix.ToString());
    }

    [Fact]
    public void NickAndUser_ToString()
    {
        var prefix = new Prefix(name: "nick", user: "user", host: null);
        Assert.Equal("nick!user", prefix.ToString());
    }

    [Fact]
    public void NickAndHost_ToString()
    {
        var prefix = new Prefix(name: "nick", user: null, host: "host");
        Assert.Equal("nick@host", prefix.ToString());
    }

    [Fact]
    public void Full_ToString()
    {
        var prefix = new Prefix(name: "nick", user: "user", host: "host");
        Assert.Equal("nick!user@host", prefix.ToString());
    }

    [Theory]
    [InlineData("nick!user@host", ParseResult.Success)]
    [InlineData("nick!@host", ParseResult.PrefixEmptyUser)]
    [InlineData("nick!user@", ParseResult.PrefixEmptyHost)]
    [InlineData("nick!@", ParseResult.PrefixEmptyHost)]
    [InlineData("nick!", ParseResult.PrefixEmptyUser)]
    [InlineData("nick@", ParseResult.PrefixEmptyHost)]
    [InlineData("!", ParseResult.PrefixEmptyUser)]
    [InlineData("@", ParseResult.PrefixEmptyHost)]
    [InlineData("", ParseResult.PrefixEmpty)]
    [InlineData("!user", ParseResult.PrefixEmptyName)]
    [InlineData("@host", ParseResult.PrefixEmptyName)]
    [InlineData("!user@host", ParseResult.PrefixEmptyName)]
    internal void ParseResultTest(string input, ParseResult expectedResult)
    {
        ParseResult result = Prefix.Parse(input, out _);
        Assert.Equal(expectedResult, result);

        if (result is ParseResult.Success)
        {
            _ = Prefix.Parse(input);
        }
        else
        {
            Assert.Throws<FormatException>(
                () => Prefix.Parse(input)
            );
        }
    }
}
