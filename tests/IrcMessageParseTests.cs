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
            Assert.Null(message.Hostmask);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void PongCommandOnly()
        {
            var message = IrcMessage.Parse("PONG");

            Assert.Null(message.Tags);
            Assert.Null(message.Hostmask);
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
            Assert.Null(message.Hostmask);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void PingSingleFlagHostmask()
        {
            var message = IrcMessage.Parse("@tag :hostmask PING");

            Assert.Collection(message.Tags,
                tag =>
                {
                    Assert.Equal("tag", tag.Key);
                    Assert.Equal("", tag.Value);
                }
            );
            Assert.Equal("hostmask", message.Hostmask);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void PingSingleTagHostmask()
        {
            var message = IrcMessage.Parse("@key=value :hostmask PING");

            Assert.Collection(message.Tags,
                tag =>
                {
                    Assert.Equal("key", tag.Key);
                    Assert.Equal("value", tag.Value);
                }
            );
            Assert.Equal("hostmask", message.Hostmask);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void MultiWordArg()
        {
            var message = IrcMessage.Parse(":hostmask PING arg a");

            Assert.Null(message.Tags);
            Assert.Equal("hostmask", message.Hostmask);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Equal("arg a", message.Arg);
            Assert.Null(message.Content);
        }

        [Fact]
        public void NoArgContent()
        {
            var message = IrcMessage.Parse(":hostmask PING :content c");

            Assert.Null(message.Tags);
            Assert.Equal("hostmask", message.Hostmask);
            Assert.Equal(IrcCommand.PING, message.Command);
            Assert.Null(message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("content c", message.Content!);
        }

        [Fact]
        public void ArgContent()
        {
            var message = IrcMessage.Parse(":hostmask PONG arg a :content c");

            Assert.Null(message.Tags);
            Assert.Equal("hostmask", message.Hostmask);
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
            Assert.Null(message.Hostmask);
            Assert.Equal(IrcCommand.CAP, message.Command);
            Assert.Equal("REQ", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("cap1 cap2", message.Content!);
        }

        [Fact]
        public void CapAck()
        {
            var message = IrcMessage.Parse(":hostmask CAP * ACK :cap1 cap2");

            Assert.Null(message.Tags);
            Assert.Equal("hostmask", message.Hostmask);
            Assert.Equal(IrcCommand.CAP, message.Command);
            Assert.Equal("* ACK", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("cap1 cap2", message.Content!);
        }

        [Fact]
        public void Command353()
        {
            var message = IrcMessage.Parse(":hostmask 353 tera = #channel :name1 name2 name3");

            Assert.Null(message.Tags);
            Assert.Equal("hostmask", message.Hostmask);
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
            Assert.Null(message.Hostmask);
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
            Assert.Null(message.Hostmask);
            Assert.Equal(IrcCommand.PRIVMSG, message.Command);
            Assert.Equal("#channel", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("ACTION", message.Content!.Ctcp);
            Assert.Equal("Text", message.Content!.Text);
        }

        [Fact]
        public void PrivMsgValueTag()
        {
            var message = IrcMessage.Parse("@key=value :hostmask PRIVMSG #channel :message");

            Assert.Collection(message.Tags,
                tag =>
                {
                    Assert.Equal("key", tag.Key);
                    Assert.Equal("value", tag.Value);
                }
            );
            Assert.Equal("hostmask", message.Hostmask);
            Assert.Equal(IrcCommand.PRIVMSG, message.Command);
            Assert.Equal("#channel", message.Arg);
            Assert.NotNull(message.Content);
            Assert.Equal("message", message.Content!);
        }
    }
}
