using System.Collections.Generic;
using Xunit;

namespace Teraa.Irc.Tests;

public class MessageToStringTests
{
    [Fact]
    public void CommandOnly()
    {
        var rawMessage = new Message(
            Command: Command.PING
        ).ToString();

        Assert.Equal("PING", rawMessage);
    }

    [Fact]
    public void Tags_Prefix_Content()
    {
        var rawMessage = new Message(
            Command: Command.PRIVMSG,
            Tags: new Tags(new Dictionary<string, string> {["tag"] = ""}),
            Prefix: new Prefix("name", null, null),
            Content: new Content("message")
        ).ToString();

        Assert.Equal("@tag :name PRIVMSG :message", rawMessage);
    }

    [Fact]
    public void Arg_Content()
    {
        var rawMessage = new Message(
            Command: Command.CAP,
            Arg: "REQ",
            Content: new Content("cap1 cap2")
        ).ToString();

        Assert.Equal("CAP REQ :cap1 cap2", rawMessage);
    }

    [Fact]
    public void Prefix_NumericCommand_Arg_Content()
    {
        var rawMessage = new Message(
            Command: (Command)353,
            Prefix: new Prefix("name", null, null),
            Arg: "tera = #channel",
            Content: new Content("name1 name2 name3")
        ).ToString();

        Assert.Equal(":name 353 tera = #channel :name1 name2 name3", rawMessage);
    }

    [Fact]
    public void Arg_CtcpContent()
    {
        var rawMessage = new Message(
            Command: Command.PRIVMSG,
            Arg: "#channel",
            Content: new Content("hi", "ACTION")
        ).ToString();

        Assert.Equal("PRIVMSG #channel :\u0001ACTION hi\u0001", rawMessage);
    }

    [Fact]
    public void Prefix_Arg_Content()
    {
        var rawMessage = new Message(
            Command: Command.CAP,
            Prefix: new Prefix("name", null, null),
            Arg: "* ACK",
            Content: new Content("cap1 cap2")
        ).ToString();

        Assert.Equal(":name CAP * ACK :cap1 cap2", rawMessage);
    }
}
