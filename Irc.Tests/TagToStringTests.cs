using System.Collections.Generic;
using Xunit;

namespace Teraa.Irc.Tests;

public class TagToStringTests
{
    [Fact]
    public void SingleTag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = "value",
        });

        Assert.Equal("key=value", tags.ToString());
    }

    [Fact]
    public void EmptyAsFlag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = "",
        });

        Assert.Equal("key", tags.ToString());
    }

    [Fact]
    public void NullAsFlag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["key"] = null!,
        });

        Assert.Equal("key", tags.ToString());
    }

    [Fact]
    public void PrefixedTag()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["+key"] = "value",
        });

        Assert.Equal("+key=value", tags.ToString());
    }

    [Fact]
    public void MultipleTags()
    {
        var tags = new Tags(new Dictionary<string, string>
        {
            ["one"] = "[one]",
            ["two"] = "",
            ["three"] = "[three]",
            ["four"] = null!,
        });

        Assert.Equal("one=[one];two;three=[three];four", tags.ToString());
    }
}
