using System;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests.Parsing;

public class MessageParserTests
{
    private readonly MessageParser _parser = new MessageParser();

    [Fact]
    public void Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

    [Fact]
    public void PingCommandOnly()
    {
        var message = _parser.Parse("PING");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PING, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void PongCommandOnly()
    {
        var message = _parser.Parse("PONG");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.PONG, message.Command);
        Assert.Null(message.Arg);
        Assert.Null(message.Content);
    }

    [Fact]
    public void PingSingleFlag()
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
    public void PingSingleFlagPrefix()
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
    public void PingSingleTagPrefix()
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
    public void MultiWordArg()
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
    public void NoArgContent()
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
        Assert.Equal("content c", message.Content!.ToString());
    }

    [Fact]
    public void ArgContent()
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
        Assert.Equal("content c", message.Content!.ToString());
    }

    [Fact]
    public void CapReq()
    {
        var message = _parser.Parse("CAP REQ :cap1 cap2");

        Assert.Null(message.Tags);
        Assert.Null(message.Prefix);
        Assert.Equal(Command.CAP, message.Command);
        Assert.Equal("REQ", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("cap1 cap2", message.Content!.ToString());
    }

    [Fact]
    public void CapAck()
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
        Assert.Equal("cap1 cap2", message.Content!.ToString());
    }

    [Fact]
    public void CommandNumeric()
    {
        var message = _parser.Parse(":name 353 tera = #channel :name1 name2 name3");

        Assert.Null(message.Tags);
        Assert.NotNull(message.Prefix);
        Assert.Equal("name", message.Prefix!.Name);
        Assert.Null(message.Prefix.User);
        Assert.Null(message.Prefix.Host);
        Assert.Equal((Command)353, message.Command);
        Assert.Equal("tera = #channel", message.Arg);
        Assert.NotNull(message.Content);
        Assert.Equal("name1 name2 name3", message.Content!.ToString());
    }

    [Fact]
    public void PrivmsgAction()
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
    public void PrivmsgMissingActionEnd()
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
    public void PrivmsgValueTag()
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
        Assert.Equal("message", message.Content!.ToString());
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
    internal void ParseResultTest(string input, MessageParser.Result expectedResult)
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
}
