using System.Collections.Generic;
using Xunit;

namespace IrcMessageParser.Tests
{
    public class IrcMessageParseTests
    {
        [Fact]
        public void PingCommandOnly()
        {
            var message = IrcMessage.Parse("PING");

            Assert.Null(message.Tags);
            Assert.Null(message.Prefix);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void PongCommandOnly()
        {
            var message = IrcMessage.Parse("PONG");

            Assert.Null(message.Tags);
            Assert.Null(message.Prefix);
            Assert.Equal(IrcCommand.PONG, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void PingSingleFlag()
        {
            var message = IrcMessage.Parse("@tag PING");

            Assert.Collection(message.Tags,
                tag =>
                {
                    Assert.Equal("tag", tag.Key);
                    Assert.Equal("", tag.Value);
                }
            );
            Assert.Null(message.Prefix);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void PingSingleFlagPrefix()
        {
            var message = IrcMessage.Parse("@tag :name PING");

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
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void PingSingleTagPrefix()
        {
            var message = IrcMessage.Parse("@key=value :name PING");

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
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void MultiWordArg()
        {
            var message = IrcMessage.Parse(":name PING arg a");

            Assert.Null(message.Tags);
            Assert.NotNull(message.Prefix);
            Assert.Equal("name", message.Prefix!.Name);
            Assert.Null(message.Prefix.User);
            Assert.Null(message.Prefix.Host);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Equal("arg a", message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void NoArgContent()
        {
            var message = IrcMessage.Parse(":name PING :content c");

            Assert.Null(message.Tags);
            Assert.NotNull(message.Prefix);
            Assert.Equal("name", message.Prefix!.Name);
            Assert.Null(message.Prefix.User);
            Assert.Null(message.Prefix.Host);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("content c", message.Content!);
        }

        [Fact]
        public void ArgContent()
        {
            var message = IrcMessage.Parse(":name PONG arg a :content c");

            Assert.Null(message.Tags);
            Assert.NotNull(message.Prefix);
            Assert.Equal("name", message.Prefix!.Name);
            Assert.Null(message.Prefix.User);
            Assert.Null(message.Prefix.Host);
            Assert.Equal(IrcCommand.PONG, message.Command);
            Assert.Equal("arg a", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("content c", message.Content!);
        }

        [Fact]
        public void CapReq()
        {
            var message = IrcMessage.Parse("CAP REQ :cap1 cap2");

            Assert.Null(message.Tags);
            Assert.Null(message.Prefix);
            Assert.Equal(IrcCommand.CAP, message.Command);
            Assert.Equal("REQ", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("cap1 cap2", message.Content!);
        }

        [Fact]
        public void CapAck()
        {
            var message = IrcMessage.Parse(":name CAP * ACK :cap1 cap2");

            Assert.Null(message.Tags);
            Assert.NotNull(message.Prefix);
            Assert.Equal("name", message.Prefix!.Name);
            Assert.Null(message.Prefix.User);
            Assert.Null(message.Prefix.Host);
            Assert.Equal(IrcCommand.CAP, message.Command);
            Assert.Equal("* ACK", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("cap1 cap2", message.Content!);
        }

        [Fact]
        public void CommandNumeric()
        {
            var message = IrcMessage.Parse(":name 353 tera = #channel :name1 name2 name3");

            Assert.Null(message.Tags);
            Assert.NotNull(message.Prefix);
            Assert.Equal("name", message.Prefix!.Name);
            Assert.Null(message.Prefix.User);
            Assert.Null(message.Prefix.Host);
            Assert.Equal((IrcCommand)353, message.Command);
            Assert.Equal("tera = #channel", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("name1 name2 name3", message.Content!);
        }

        [Fact]
        public void PrivmsgAction()
        {
            var message = IrcMessage.Parse("PRIVMSG #channel :\u0001ACTION Text\u0001");

            Assert.Null(message.Tags);
            Assert.Null(message.Prefix);
            Assert.Equal(IrcCommand.PRIVMSG, message.Command);
            Assert.Equal("#channel", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("ACTION", message.Content!.Ctcp);
            Assert.Equal("Text", message.Content!.Text);
        }

        [Fact]
        public void PrivmsgMissingActionEnd()
        {
            var message = IrcMessage.Parse("PRIVMSG #channel :\u0001ACTION Text");

            Assert.Null(message.Tags);
            Assert.Null(message.Prefix);
            Assert.Equal(IrcCommand.PRIVMSG, message.Command);
            Assert.Equal("#channel", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("ACTION", message.Content!.Ctcp);
            Assert.Equal("Text", message.Content!.Text);
        }

        [Fact]
        public void PrivMsgValueTag()
        {
            var message = IrcMessage.Parse("@key=value :name PRIVMSG #channel :message");

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
            Assert.Equal(IrcCommand.PRIVMSG, message.Command);
            Assert.Equal("#channel", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("message", message.Content!);
        }
    }
}
