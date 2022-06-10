using System;
using System.Collections.Generic;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests;

public class MessageParserTests
{
    private readonly MessageParser _parser = new MessageParser();

    [Fact]
    public void Parse_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

    [Fact]
    public void Parse_PingCommandOnly()
    {
        var message = _parser.Parse("PING");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void Parse_PongCommandOnly()
    {
        var message = _parser.Parse("PONG");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PONG, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void Parse_PingSingleFlag()
    {
        var message = _parser.Parse("@tag PING");

        Assert.Collection(message.Tags!,
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
    public void Parse_PingSingleFlagPrefix()
    {
        var message = _parser.Parse("@tag :name PING");

        Assert.Collection(message.Tags!,
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
    public void Parse_PingSingleTagPrefix()
    {
        var message = _parser.Parse("@key=value :name PING");

        Assert.Collection(message.Tags!,
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
    public void Parse_MultiWordArg()
    {
        var message = _parser.Parse(":name PING arg a");

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
    public void Parse_NoArgContent()
    {
        var message = _parser.Parse(":name PING :content c");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.NotNull(message.Content);
        Assert.Null(message.Content!.Ctcp);
        Assert.Equal("content c", message.Content.Text);
    }

    [Fact]
    public void Parse_ArgContent()
    {
        var message = _parser.Parse(":name PONG arg a :content c");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.PONG, message.Command);
        Assert.Equal("arg a", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Null(message.Content!.Ctcp);
        Assert.Equal("content c", message.Content.Text);
    }

    [Fact]
    public void Parse_CapReq()
    {
        var message = _parser.Parse("CAP REQ :cap1 cap2");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.CAP, message.Command);
        Assert.Equal("REQ", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Null(message.Content!.Ctcp);
        Assert.Equal("cap1 cap2", message.Content.Text);
    }

    [Fact]
    public void Parse_CapAck()
    {
        var message = _parser.Parse(":name CAP * ACK :cap1 cap2");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal(Command.CAP, message.Command);
        Assert.Equal("* ACK", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Null(message.Content!.Ctcp);
        Assert.Equal("cap1 cap2", message.Content.Text);
    }

    [Fact]
    public void Parse_CommandNumeric()
    {
        var message = _parser.Parse(":name 353 tera = #channel :name1 name2 name3");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal((Command) 353, message.Command);
        Assert.Equal("tera = #channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Null(message.Content!.Ctcp);
        Assert.Equal("name1 name2 name3", message.Content.Text);
    }

    [Fact]
    public void Parse_PrivmsgAction()
    {
        var message = _parser.Parse("PRIVMSG #channel :\u0001ACTION Text\u0001");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PRIVMSG, message.Command);
        Assert.Equal("#channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("ACTION", message.Content!.Ctcp);
        Assert.Equal("Text", message.Content!.Text);
    }

    [Fact]
    public void Parse_PrivmsgMissingActionEnd()
    {
        var message = _parser.Parse("PRIVMSG #channel :\u0001ACTION Text");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PRIVMSG, message.Command);
        Assert.Equal("#channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("ACTION", message.Content!.Ctcp);
        Assert.Equal("Text", message.Content!.Text);
    }

    [Fact]
    public void Parse_PrivmsgValueTag()
    {
        var message = _parser.Parse("@key=value :nick!user@host PRIVMSG #channel :message");

        Assert.Collection(message.Tags!,
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
        Assert.Null(message.Content!.Ctcp);
        Assert.Equal("message", message.Content.Text);
    }

    [Theory]
    [InlineData("PING", MessageParser.Result.Success)]
    [InlineData("", MessageParser.Result.Empty)]
    [InlineData("@ ", MessageParser.Result.InvalidTags)]
    [InlineData(": ", MessageParser.Result.InvalidPrefix)]
    [InlineData("0", MessageParser.Result.InvalidCommand)]
    [InlineData("0 ", MessageParser.Result.InvalidCommand)]
    [InlineData("PING :\u0001ACTION", MessageParser.Result.InvalidContent)]
    [InlineData("@tag", MessageParser.Result.NoCommandMissingTagsEnding)]
    [InlineData("@tag ", MessageParser.Result.NoCommandAfterTagsEnding)]
    [InlineData("@tag :name", MessageParser.Result.NoCommandMissingPrefixEnding)]
    [InlineData("@tag :name ", MessageParser.Result.NoCommandAfterPrefixEnding)]
    [InlineData("@tag :name PING ", MessageParser.Result.TrailingSpaceAfterCommand)]
    internal void Parse_ResultTest(string input, MessageParser.Result expectedResult)
    {
        MessageParser.Result result = _parser.Parse(input, out _);
        Assert.Equal(expectedResult, result);

        if (result is MessageParser.Result.Success)
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

    [Fact]
    public void ToString_CommandOnly()
    {
        var message = new Message(
            Command: Command.PING);

        string rawMessage = _parser.ToString(message);

        Assert.Equal("PING", rawMessage);
    }

    [Fact]
    public void ToString_TagsPrefixContent()
    {
        var message = new Message(
            Command: Command.PRIVMSG,
            Tags: new Tags(new Dictionary<string, string> {["tag"] = ""}),
            Prefix: new Prefix("name", null, null),
            Content: new Content("message"));

        string rawMessage = _parser.ToString(message);

        Assert.Equal("@tag :name PRIVMSG :message", rawMessage);
    }

    [Fact]
    public void ToString_ArgContent()
    {
        var message = new Message(
            Command: Command.CAP,
            Arg: "REQ",
            Content: new Content("cap1 cap2"));

        string rawMessage = _parser.ToString(message);

        Assert.Equal("CAP REQ :cap1 cap2", rawMessage);
    }

    [Fact]
    public void ToString_PrefixNumericCommandArgContent()
    {
        var message = new Message(
            Command: (Command) 353,
            Prefix: new Prefix("name", null, null),
            Arg: "tera = #channel",
            Content: new Content("name1 name2 name3"));

        string rawMessage = _parser.ToString(message);

        Assert.Equal(":name 353 tera = #channel :name1 name2 name3", rawMessage);
    }

    [Fact]
    public void ToString_ArgCtcpContent()
    {
        var message = new Message(
            Command: Command.PRIVMSG,
            Arg: "#channel",
            Content: new Content("hi", "ACTION"));

        string rawMessage = _parser.ToString(message);

        Assert.Equal("PRIVMSG #channel :\u0001ACTION hi\u0001", rawMessage);
    }

    [Fact]
    public void ToString_PrefixArgContent()
    {
        var message = new Message(
            Command: Command.CAP,
            Prefix: new Prefix("name", null, null),
            Arg: "* ACK",
            Content: new Content("cap1 cap2"));

        string rawMessage = _parser.ToString(message);

        Assert.Equal(":name CAP * ACK :cap1 cap2", rawMessage);
    }
}
