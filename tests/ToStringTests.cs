using System.Collections.Generic;
using Xunit;

namespace IrcMessageParser.Tests
{
    public class ToStringTests
    {
        [Fact]
        public void ToString1()
        {
            var message = new IrcMessage
            {
                Command = IrcCommand.PING,
            };

            Assert.Equal("PING", message.ToString());
        }

        [Fact]
        public void ToString2()
        {
            var message = new IrcMessage
            {
                Tags = new Dictionary<string, string>
                {
                    ["tag"] = ""
                },
                Hostmask = "hostmask",
                Command = IrcCommand.PRIVMSG,
                Content = new("message"),
            };

            Assert.Equal("@tag :hostmask PRIVMSG :message", message.ToString());
        }

        [Fact]
        public void ToString3()
        {
            var message = new IrcMessage
            {
                Command = IrcCommand.CAP,
                Arg = "REQ",
                Content = new("twitch.tv/tags twitch.tv/commands"),
            };
            
            Assert.Equal("CAP REQ :twitch.tv/tags twitch.tv/commands", message.ToString());
        }

        [Fact]
        public void ToString4()
        {
            var message = new IrcMessage
            {
                Hostmask = "hostmask",
                Command = (IrcCommand)353,
                Arg = "tera = #channel",
                Content = new("name1 name2 name3"),
            };
            
            Assert.Equal(":hostmask 353 tera = #channel :name1 name2 name3", message.ToString());
        }

        [Fact]
        public void ToString5()
        {
            var message = new IrcMessage
            {
                Command = IrcCommand.PRIVMSG,
                Arg = "#channel",
                Content = new("hi", "ACTION"),
            };
            
            Assert.Equal("PRIVMSG #channel :\u0001ACTION hi\u0001", message.ToString());
        }

        [Fact]
        public void ToString6()
        {
            var message = new IrcMessage
            {
                Hostmask = "tmi.twitch.tv",
                Command = IrcCommand.CAP,
                Arg = "* ACK",
                Content = new("twitch.tv/tags twitch.tv/commands"),
            };

            Assert.Equal(":tmi.twitch.tv CAP * ACK :twitch.tv/tags twitch.tv/commands", message.ToString());
        }
    }
}
