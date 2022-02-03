using System;
using Xunit;

namespace Teraa.Irc.Tests;

public class CommandParserTests
{
    [Theory]
    [InlineData("100", (Command)100)]
    [InlineData("005", (Command)005)]
    [InlineData("999", (Command)999)]
    [InlineData("CAP", Command.CAP)]
    [InlineData("GLOBALUSERSTATE", Command.GLOBALUSERSTATE)]
    [InlineData("PING", Command.PING)]
    [InlineData("PONG", Command.PONG)]
    [InlineData("PRIVMSG", Command.PRIVMSG)]
    [InlineData("cap", Command.CAP)]
    [InlineData("globaluserstate", Command.GLOBALUSERSTATE)]
    [InlineData("ping", Command.PING)]
    [InlineData("pong", Command.PONG)]
    [InlineData("privmsg", Command.PRIVMSG)]
    public void Parse_Successful(string input, Command expected)
    {
        var parsed = CommandParser.Parse(input);
        Assert.Equal(expected, parsed);
    }

    [Theory]
    [InlineData(Command.ADMIN, "ADMIN")]
    [InlineData(Command.PING, "PING")]
    [InlineData(Command.CAP, "CAP")]
    [InlineData(Command.RPL_TRACELINK, "200")]
    [InlineData(Command.RPL_ADMINEMAIL, "259")]
    [InlineData((Command)100, "100")]
    [InlineData((Command)999, "999")]
    public void ToStringTest(Command command, string expected)
    {
        var actual = CommandParser.ToString(command);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("100", FailResult.None)]
    [InlineData("", FailResult.CommandEmpty)]
    [InlineData("1", FailResult.CommandFormat)]
    [InlineData("10", FailResult.CommandFormat)]
    [InlineData("1000", FailResult.CommandFormat)]
    [InlineData("-10", FailResult.CommandFormat)]
    [InlineData("-100", FailResult.CommandFormat)]
    [InlineData("invalid", FailResult.CommandFormat)]
    internal void ParseResultTest(string input, FailResult expectedResult)
    {
        FailResult result = CommandParser.Parse(input, out _);
        Assert.Equal(expectedResult, result);

        if (result is FailResult.None)
        {
            _ = CommandParser.Parse(input);
        }
        else
        {
            Assert.Throws<FormatException>(
                () => CommandParser.Parse(input)
            );
        }
    }
}
