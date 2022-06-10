using Xunit;

namespace Teraa.Irc.Tests;

public class ContentTests
{
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
