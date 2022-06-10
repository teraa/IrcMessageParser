using System;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests;

public class PrefixTests
{
    private readonly PrefixParser _parser = new PrefixParser();

    [Fact]
    public void Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

    [Fact]
    public void NameOnly_Parse()
    {
        var raw = "servername";
        var prefix = _parser.Parse(raw);

        Assert.Equal(raw, prefix.Name);
        Assert.Null(prefix.User);
        Assert.Null(prefix.Host);
    }

    [Fact]
    public void NickAndUser_Parse()
    {
        var raw = "nick!user";
        var prefix = _parser.Parse(raw);

        Assert.Equal("nick", prefix.Name);
        Assert.Equal("user", prefix.User);
        Assert.Null(prefix.Host);
    }

    [Fact]
    public void NickAndHost_Parse()
    {
        var raw = "nick@host";
        var prefix = _parser.Parse(raw);

        Assert.Equal("nick", prefix.Name);
        Assert.Null(prefix.User);
        Assert.Equal("host", prefix.Host);
    }

    [Fact]
    public void Full_Parse()
    {
        var raw = "nick!user@host";
        var prefix = _parser.Parse(raw);

        Assert.Equal("nick", prefix.Name);
        Assert.Equal("user", prefix.User);
        Assert.Equal("host", prefix.Host);
    }

    [Fact]
    public void NameOnly_ToString()
    {
        var prefix = new Prefix(Name: "servername", User: null, Host: null);
        Assert.Equal("servername", prefix.ToString());
    }

    [Fact]
    public void NickAndUser_ToString()
    {
        var prefix = new Prefix(Name: "nick", User: "user", Host: null);
        Assert.Equal("nick!user", prefix.ToString());
    }

    [Fact]
    public void NickAndHost_ToString()
    {
        var prefix = new Prefix(Name: "nick", User: null, Host: "host");
        Assert.Equal("nick@host", prefix.ToString());
    }

    [Fact]
    public void Full_ToString()
    {
        var prefix = new Prefix(Name: "nick", User: "user", Host: "host");
        Assert.Equal("nick!user@host", prefix.ToString());
    }

    [Theory]
    [InlineData("nick!user@host", PrefixParser.Result.Success)]
    [InlineData("nick!@host", PrefixParser.Result.EmptyUser)]
    [InlineData("nick!user@", PrefixParser.Result.EmptyHost)]
    [InlineData("nick!@", PrefixParser.Result.EmptyHost)]
    [InlineData("nick!", PrefixParser.Result.EmptyUser)]
    [InlineData("nick@", PrefixParser.Result.EmptyHost)]
    [InlineData("!", PrefixParser.Result.EmptyUser)]
    [InlineData("@", PrefixParser.Result.EmptyHost)]
    [InlineData("", PrefixParser.Result.Empty)]
    [InlineData("!user", PrefixParser.Result.EmptyName)]
    [InlineData("@host", PrefixParser.Result.EmptyName)]
    [InlineData("!user@host", PrefixParser.Result.EmptyName)]
    internal void ParseResultTest(string input, PrefixParser.Result expectedResult)
    {
        PrefixParser.Result result = PrefixParser.Parse(input, out _);
        Assert.Equal(expectedResult, result);

        if (result is PrefixParser.Result.Success)
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
