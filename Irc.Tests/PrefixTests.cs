using Xunit;
using static Teraa.Irc.Prefix;

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
    [InlineData("nick!user@host", ParseStatus.Success)]
    [InlineData("nick!@host", ParseStatus.FailEmptyUser)]
    [InlineData("nick!user@", ParseStatus.FailEmptyHost)]
    [InlineData("nick!@", ParseStatus.FailEmptyHost)]
    [InlineData("nick!", ParseStatus.FailEmptyUser)]
    [InlineData("nick@", ParseStatus.FailEmptyHost)]
    [InlineData("!", ParseStatus.FailEmptyUser)]
    [InlineData("@", ParseStatus.FailEmptyHost)]
    [InlineData("", ParseStatus.FailEmpty)]
    [InlineData("!user", ParseStatus.FailEmptyName)]
    [InlineData("@host", ParseStatus.FailEmptyName)]
    [InlineData("!user@host", ParseStatus.FailEmptyName)]
    internal void ParseStatusTest(string input, ParseStatus expectedStatus)
    {
        var status = Prefix.Parse(input, out _);
        Assert.Equal(expectedStatus, status);
    }
}
