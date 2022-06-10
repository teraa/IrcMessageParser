using System;
using Teraa.Irc.Parsing;
using Xunit;

namespace Teraa.Irc.Tests.Parsing;

public class CommandParserTests
{
    private readonly CommandParser _parser = new CommandParser();

    [Fact]
    public void Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _parser.Parse(null!));
    }

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
        var parsed = _parser.Parse(input);
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
    [InlineData("100", CommandParser.ParseResult.Success)]
    [InlineData("", CommandParser.ParseResult.CommandEmpty)]
    [InlineData("1", CommandParser.ParseResult.CommandFormat)]
    [InlineData("10", CommandParser.ParseResult.CommandFormat)]
    [InlineData("1000", CommandParser.ParseResult.CommandFormat)]
    [InlineData("-10", CommandParser.ParseResult.CommandFormat)]
    [InlineData("-100", CommandParser.ParseResult.CommandFormat)]
    [InlineData("invalid", CommandParser.ParseResult.CommandFormat)]
    internal void ParseResultTest(string input, CommandParser.ParseResult expectedResult)
    {
        CommandParser.ParseResult result = CommandParser.Parse(input, out _);
        Assert.Equal(expectedResult, result);

        if (result is CommandParser.ParseResult.Success)
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
