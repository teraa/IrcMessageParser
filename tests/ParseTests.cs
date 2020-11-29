using System.Collections.Generic;
using Xunit;

namespace Twitch.Irc.Tests
{
    public class ParseTests
    {
        [Theory]
        [InlineData("PING", null, null, IrcCommand.PING, null, null, null)]
        [InlineData("PONG", null, null, IrcCommand.PONG, null, null, null)]
        [InlineData("PING arg", null, null, IrcCommand.PING, "arg", null, null)]
        [InlineData("PING :content", null, null, IrcCommand.PING, null, "content", false)]
        [InlineData(":hostmask PONG arg :content", null, "hostmask", IrcCommand.PONG, "arg", "content", false)]
        [InlineData("CAP REQ :twitch.tv/tags twitch.tv/commands", null, null, IrcCommand.CAP, "REQ", "twitch.tv/tags twitch.tv/commands", false)]
        [InlineData(":hostmask 353 tera = #channel :name1 name2 name3",
            null, "hostmask", (IrcCommand)353, "tera = #channel", "name1 name2 name3", false)]
        public void ParseTest(
            string raw,
            Dictionary<string, string> tags,
            string? hostmask,
            IrcCommand command,
            string? arg,
            string? content,
            bool? isAction,
            bool checkTags = false)
        {
            var actual = IrcMessage.Parse(raw);

            Assert.Equal(arg, actual.Arg);
            Assert.Equal(command, actual.Command);
            Assert.Equal(content, actual.Content);
            Assert.Equal(hostmask, actual.Hostmask);
            Assert.Equal(isAction, actual.IsAction);

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
