using System;
using Xunit;

namespace IrcMessageParser.Tests;

    public class IrcCommandParserTests
    {
        [Theory]
        [InlineData("100", (IrcCommand)100)]
        [InlineData("005", (IrcCommand)005)]
        [InlineData("999", (IrcCommand)999)]
        [InlineData("CAP", IrcCommand.CAP)]
        [InlineData("GLOBALUSERSTATE", IrcCommand.GLOBALUSERSTATE)]
        [InlineData("PING", IrcCommand.PING)]
        [InlineData("PONG", IrcCommand.PONG)]
        [InlineData("PRIVMSG", IrcCommand.PRIVMSG)]
        [InlineData("cap", IrcCommand.CAP)]
        [InlineData("globaluserstate", IrcCommand.GLOBALUSERSTATE)]
        [InlineData("ping", IrcCommand.PING)]
        [InlineData("pong", IrcCommand.PONG)]
        [InlineData("privmsg", IrcCommand.PRIVMSG)]
        public void Parse_Successful(string input, IrcCommand expected)
        {
            var parsed = IrcCommandParser.Parse(input);
            Assert.Equal(expected, parsed);
        }


        [Theory]
        [InlineData("1")]
        [InlineData("10")]
        [InlineData("1000")]
        [InlineData("-10")]
        [InlineData("-100")]
        [InlineData("invalid")]
        public void Parse_Throws(string input)
        {
            Assert.Throws<FormatException>(
                () => IrcCommandParser.Parse(input)
            );
        }

        [Theory]
        [InlineData(IrcCommand.ADMIN, "ADMIN")]
        [InlineData(IrcCommand.PING, "PING")]
        [InlineData(IrcCommand.CAP, "CAP")]
        [InlineData(IrcCommand.RPL_TRACELINK, "200")]
        [InlineData(IrcCommand.RPL_ADMINEMAIL, "259")]
        [InlineData((IrcCommand)100, "100")]
        [InlineData((IrcCommand)999, "999")]
        public void ToStringTest(IrcCommand command, string expected)
        {
            var actual = IrcCommandParser.ToString(command);
            Assert.Equal(expected, actual);
        }
    }
