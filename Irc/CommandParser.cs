using System;
using JetBrains.Annotations;

namespace Teraa.Irc;

/// <summary>
///     Class for parsing <see cref="Command"/> values.
/// </summary>
[PublicAPI]
public static class CommandParser
{
    private const Command s_maxNumeric = (Command)999;

    /// <inheritdoc cref="Parse(ReadOnlySpan{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public static Command Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <summary>
    ///     Parses the <see cref="Command"/> from <paramref name="input"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <returns><see cref="Command"/>.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public static Command Parse(ReadOnlySpan<char> input)
    {
        ParseResult parseResult = Parse(input, out Command result);
        if (parseResult is ParseResult.Success)
            return result;

        throw new FormatException(parseResult.ParseResultToString());
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, out Command)"/>
    public static bool TryParse(string? input, out Command result)
        => TryParse(input.AsSpan(), out result);

    /// <summary>
    ///     Parses the <see cref="Command"/> from <paramref name="input"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed command if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out Command result)
        => Parse(input, out result) == ParseResult.Success;

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

    internal static ParseResult Parse(ReadOnlySpan<char> input, out Command result)
    {
        result = 0;

        if (input.IsEmpty)
            return ParseResult.CommandEmpty;

        if (input.Length == 3 && ushort.TryParse(input, out ushort numeric))
            result = (Command)numeric;
        else if (Enum.TryParse(input, true, out Command command)
            && command is > s_maxNumeric
            && (input[0] is (< '0' or > '9')))
            result = command;
        else
            return ParseResult.CommandFormat;

        return ParseResult.Success;
    }
}
