using System.Collections.Generic;
using Xunit;

namespace Twitch.Irc.Tests
{
    public class ParseTests
    {
        [Theory]
        [InlineData("PING", null, null, IrcCommand.PING, null, null, null)]
        [InlineData("PONG", null, null, IrcCommand.PONG, null, null, null)]

        [InlineData(":hostmask PING arg a",
            null, "hostmask", IrcCommand.PING, "arg a", null, null)]

        [InlineData(":hostmask PING :content c",
            null, "hostmask", IrcCommand.PING, null, "content c", false)]

        [InlineData(":hostmask PONG arg a :content c",
            null, "hostmask", IrcCommand.PONG, "arg a", "content c", false)]

        [InlineData("CAP REQ :twitch.tv/tags twitch.tv/commands",
            null, null, IrcCommand.CAP, "REQ", "twitch.tv/tags twitch.tv/commands", false)]

        [InlineData(":hostmask 353 tera = #channel :name1 name2 name3",
            null, "hostmask", (IrcCommand)353, "tera = #channel", "name1 name2 name3", false)]

        public void ParseTest(
            string raw,
            Dictionary<string, string> tags,
            string? hostmask,
            IrcCommand command,
            string? arg,
            string? contentText,
            bool? isAction,
            bool checkTags = false)
        {
            var actual = IrcMessage.Parse(raw);

            (string Text, bool IsAction)? content;
            if (contentText is not null && isAction.HasValue)
                content = (contentText, isAction.Value);
            else
                content = null;

            Assert.Equal(hostmask, actual.Hostmask);
            Assert.Equal(command, actual.Command);
            Assert.Equal(arg, actual.Arg);
            Assert.Equal(content, actual.Content);

            Assert.Equal(tags is null, actual.Tags is null);

            if (checkTags)
            {
                Assert.Equal(tags!.Count, actual.Tags!.Count);
                foreach (var (k, _) in tags)
                {
                    Assert.True(actual.Tags.ContainsKey(k));
                    Assert.Equal(tags[k], actual.Tags[k]);
                }
            }
        }
    }
}
