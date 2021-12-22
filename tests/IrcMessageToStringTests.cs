using System.Collections.Generic;
using Xunit;

namespace IrcMessageParser.Tests;

    public class IrcMessageToStringTests
    {
        [Fact]
        public void CommandOnly()
        {
            var rawMessage = new IrcMessage
            {
                Command = IrcCommand.PING,
            }.ToString();

            Assert.Equal("PING", rawMessage);
        }

        [Fact]
        public void Tags_Prefix_Content()
        {
            var rawMessage = new IrcMessage
            {
                Tags = new Dictionary<string, string>
                {
                    ["tag"] = ""
                },
                Prefix = new("name", null, null),
                Command = IrcCommand.PRIVMSG,
                Content = new("message"),
            }.ToString();

            Assert.Equal("@tag :name PRIVMSG :message", rawMessage);
        }

        [Fact]
        public void Arg_Content()
        {
            var rawMessage = new IrcMessage
            {
                Command = IrcCommand.CAP,
                Arg = "REQ",
                Content = new("cap1 cap2"),
            }.ToString();

            Assert.Equal("CAP REQ :cap1 cap2", rawMessage);
        }

        [Fact]
        public void Prefix_NumericCommand_Arg_Content()
        {
            var rawMessage = new IrcMessage
            {
                Prefix = new("name", null, null),
                Command = (IrcCommand)353,
                Arg = "tera = #channel",
                Content = new("name1 name2 name3"),
            }.ToString();

            Assert.Equal(":name 353 tera = #channel :name1 name2 name3", rawMessage);
        }

        [Fact]
        public void Arg_CtcpContent()
        {
            var rawMessage = new IrcMessage
            {
                Command = IrcCommand.PRIVMSG,
                Arg = "#channel",
                Content = new("hi", "ACTION"),
            }.ToString();

            Assert.Equal("PRIVMSG #channel :\u0001ACTION hi\u0001", rawMessage);
        }

        [Fact]
        public void Prefix_Arg_Content()
        {
            var rawMessage = new IrcMessage
            {
                Prefix = new("name", null, null),
                Command = IrcCommand.CAP,
                Arg = "* ACK",
                Content = new("cap1 cap2"),
            }.ToString();

            Assert.Equal(":name CAP * ACK :cap1 cap2", rawMessage);
        }
    }
