using System;

namespace Teraa.Irc;

/// <summary>
///     Class for parsing <see cref="Command"/> values.
/// </summary>
public static class CommandParser
{
    private const Command s_maxNumeric = (Command)999;

    /// <summary>
    ///     Parses the <see cref="Command"/> from <paramref name="input"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <returns><see cref="Command"/>.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public static Command Parse(ReadOnlySpan<char> input)
    {
        if (input.Length == 3 && ushort.TryParse(input, out var numeric))
            return (Command)numeric;

        if (Enum.TryParse<Command>(input, true, out var command)
            && command is > s_maxNumeric
            && (input[0] is (< '0' or > '9')))
                return command;

        throw new FormatException($"Invalid command format: {input.ToString()}");
    }

    /// <summary>
    ///     Returns the <see cref="string"/> representation of the <see cref="Command"/>
    /// </summary>
    /// <param name="command">Input command.</param>
    /// <returns><see cref="string"/> representing the command.</returns>
    public static string ToString(Command command)
    {
        if (command is > s_maxNumeric)
            return command.ToString();

        return ((ushort)command).ToString("d3");
    }
}
