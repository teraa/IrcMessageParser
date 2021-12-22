using System.Collections.Generic;
using Xunit;

namespace Teraa.Irc.Tests;

public class MessageParseTests
{
    [Fact]
    public void PingCommandOnly()
    {
        var message = Message.Parse("PING");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void PongCommandOnly()
    {
        var message = Message.Parse("PONG");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PONG, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void PingSingleFlag()
    {
        var message = Message.Parse("@tag PING");

        Assert.Collection(message.Tags,
            tag =>
            {
                Assert.Equal("tag", tag.Key);
                Assert.Equal("", tag.Value);
            }
        );
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void PingSingleFlagPrefix()
    {
        var message = Message.Parse("@tag :name PING");

        Assert.Collection(message.Tags,
            tag =>
            {
                Assert.Equal("tag", tag.Key);
                Assert.Equal("", tag.Value);
            }
        );
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void PingSingleTagPrefix()
    {
        var message = Message.Parse("@key=value :name PING");

        Assert.Collection(message.Tags,
            tag =>
            {
                Assert.Equal("key", tag.Key);
                Assert.Equal("value", tag.Value);
            }
        );
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void MultiWordArg()
    {
        var message = Message.Parse(":name PING arg a");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.PING, message.Command);
        Assert.Equal("arg a", message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void NoArgContent()
    {
        var message = Message.Parse(":name PING :content c");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("content c", message.Content!);
    }

    [Fact]
    public void ArgContent()
    {
        var message = Message.Parse(":name PONG arg a :content c");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.PONG, message.Command);
        Assert.Equal("arg a", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("content c", message.Content!);
    }

    [Fact]
    public void CapReq()
    {
        var message = Message.Parse("CAP REQ :cap1 cap2");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.CAP, message.Command);
        Assert.Equal("REQ", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("cap1 cap2", message.Content!);
    }

    [Fact]
    public void CapAck()
    {
        var message = Message.Parse(":name CAP * ACK :cap1 cap2");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.CAP, message.Command);
        Assert.Equal("* ACK", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("cap1 cap2", message.Content!);
    }

    [Fact]
    public void CommandNumeric()
    {
        var message = Message.Parse(":name 353 tera = #channel :name1 name2 name3");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal((Command)353, message.Command);
        Assert.Equal("tera = #channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("name1 name2 name3", message.Content!);
    }

    [Fact]
    public void PrivmsgAction()
    {
        var message = Message.Parse("PRIVMSG #channel :\u0001ACTION Text\u0001");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PRIVMSG, message.Command);
        Assert.Equal("#channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("ACTION", message.Content!.Ctcp);
        Assert.Equal("Text", message.Content!.Text);
    }

    [Fact]
    public void PrivmsgMissingActionEnd()
    {
        var message = Message.Parse("PRIVMSG #channel :\u0001ACTION Text");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PRIVMSG, message.Command);
        Assert.Equal("#channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("ACTION", message.Content!.Ctcp);
        Assert.Equal("Text", message.Content!.Text);
    }

    [Fact]
    public void PrivMsgValueTag()
    {
        var message = Message.Parse("@key=value :nick!user@host PRIVMSG #channel :message");

        Assert.Collection(message.Tags,
            tag =>
            {
                Assert.Equal("key", tag.Key);
                Assert.Equal("value", tag.Value);
            }
        );
        Assert.NotNull(message.Prefix);
        Assert.Equal("nick", message.Prefix!.Name);
        Assert.Equal("user", message.Prefix.User);
        Assert.Equal("host", message.Prefix.Host);
        Assert.Equal(Command.PRIVMSG, message.Command);
        Assert.Equal("#channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("message", message.Content!);
    }
}
