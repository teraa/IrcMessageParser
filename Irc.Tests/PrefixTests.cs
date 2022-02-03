using System;
using Xunit;

namespace Teraa.Irc.Tests;

public class PrefixTests
{
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
    [InlineData("nick!user@host", FailResult.None)]
    [InlineData("nick!@host", FailResult.PrefixEmptyUser)]
    [InlineData("nick!user@", FailResult.PrefixEmptyHost)]
    [InlineData("nick!@", FailResult.PrefixEmptyHost)]
    [InlineData("nick!", FailResult.PrefixEmptyUser)]
    [InlineData("nick@", FailResult.PrefixEmptyHost)]
    [InlineData("!", FailResult.PrefixEmptyUser)]
    [InlineData("@", FailResult.PrefixEmptyHost)]
    [InlineData("", FailResult.PrefixEmpty)]
    [InlineData("!user", FailResult.PrefixEmptyName)]
    [InlineData("@host", FailResult.PrefixEmptyName)]
    [InlineData("!user@host", FailResult.PrefixEmptyName)]
    internal void ParseStatusTest(string input, FailResult expectedStatus)
    {
        var status = Prefix.Parse(input, out _);
        Assert.Equal(expectedStatus, status);

        var success = Prefix.TryParse(input, out _);
        Assert.Equal(status is FailResult.None, success);

        if (status is FailResult.None)
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
