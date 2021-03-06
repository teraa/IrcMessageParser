using Xunit;

namespace IrcMessageParser.Tests
{
    public class MessageParseTests
    {
        [Theory]
        [InlineData("PING", false, null, IrcCommand.PING, null, null, null)]
        [InlineData("PONG", false, null, IrcCommand.PONG, null, null, null)]

        [InlineData("@tag PING",
            true, null, IrcCommand.PING, null, null, null)]

        [InlineData("@tag :hostmask PING",
            true, "hostmask", IrcCommand.PING, null, null, null)]

        [InlineData("@key=value :hostmask PING",
            true, "hostmask", IrcCommand.PING, null, null, null)]

        [InlineData(":hostmask PING arg a",
            false, "hostmask", IrcCommand.PING, "arg a", null, null)]

        [InlineData(":hostmask PING :content c",
            false, "hostmask", IrcCommand.PING, null, "content c", null)]

        [InlineData(":hostmask PONG arg a :content c",
            false, "hostmask", IrcCommand.PONG, "arg a", "content c", null)]

        [InlineData("CAP REQ :twitch.tv/tags twitch.tv/commands",
            false, null, IrcCommand.CAP, "REQ", "twitch.tv/tags twitch.tv/commands", null)]

        [InlineData(":tmi.twitch.tv CAP * ACK :twitch.tv/tags twitch.tv/commands",
            false, "tmi.twitch.tv", IrcCommand.CAP, "* ACK", "twitch.tv/tags twitch.tv/commands", null)]

        [InlineData(":hostmask 353 tera = #channel :name1 name2 name3",
            false, "hostmask", (IrcCommand)353, "tera = #channel", "name1 name2 name3", null)]

        [InlineData("PRIVMSG #channel :\u0001ACTION Test",
            false, null, IrcCommand.PRIVMSG, "#channel", "Test", "ACTION")]

        [InlineData("PRIVMSG #channel :\u0001ACTION Test\u0001",
            false, null, IrcCommand.PRIVMSG, "#channel", "Test", "ACTION")]

        public void ParseTest(
            string raw,
            bool hasTags,
            string? hostmask,
            IrcCommand command,
            string? arg,
            string? contentText,
            string? contentCtcp)
        {
            var actual = IrcMessage.Parse(raw);

            MessageContent? content;
            if (contentText is not null)
                content = new MessageContent(contentText, contentCtcp);
            else
                content = null;

            Assert.Equal(hostmask, actual.Hostmask);
            Assert.Equal(command, actual.Command);
            Assert.Equal(arg, actual.Arg);
            Assert.Equal(content, actual.Content);
            Assert.Equal(hasTags, actual.Tags is not null);
        }

        [Fact]
        public void ParseTest_Tags()
        {
            var raw = "@key=value :hostmask PRIVMSG #channel :message";
            var msg = IrcMessage.Parse(raw);

            Assert.NotNull(msg.Tags);
            Assert.Single(msg.Tags);
            Assert.Equal("value", msg.Tags!["key"]);
            Assert.Equal("hostmask", msg.Hostmask);
            Assert.Equal(IrcCommand.PRIVMSG, msg.Command);
            Assert.Equal("#channel", msg.Arg);
            Assert.Equal(new MessageContent("message"), msg.Content);
        }
    }
}
