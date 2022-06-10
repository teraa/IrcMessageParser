using Xunit;

namespace Teraa.Irc.Tests;

public class PrefixTests
{
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
}
