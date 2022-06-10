using System;
using JetBrains.Annotations;

namespace Teraa.Irc.Parsing;

/// <summary>
///     <see cref="Command"/> parser.
/// </summary>
[PublicAPI]
public interface ICommandParser
{
    /// <summary>
    ///     Parses the <see cref="Command"/> from <paramref name="input"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <returns><see cref="Command"/>.</returns>
    Command Parse(ReadOnlySpan<char> input);

    /// <inheritdoc cref="Parse(System.ReadOnlySpan{char})"/>
    Command Parse(string input);

    /// <summary>
    ///     Parses the <see cref="Command"/> from <paramref name="input"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed command if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    bool TryParse(ReadOnlySpan<char> input, out Command result);

    /// <inheritdoc cref="TryParse(System.ReadOnlySpan{char},out Teraa.Irc.Command)"/>
    bool TryParse(string? input, out Command result);

    /// <summary>
    ///     Returns the <see cref="string"/> representation of the <see cref="Command"/>
    /// </summary>
    /// <param name="command">Command.</param>
    /// <returns><see cref="string"/> representing the command.</returns>
    string ToString(Command command);
}

/// <inheritdoc />
[PublicAPI]
public class CommandParser : ICommandParser
{
    private const Command s_maxNumeric = (Command)999;

    /// <inheritdoc />
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public Command Parse(ReadOnlySpan<char> input)
    {
        ParseResult parseResult = Parse(input, out Command result);
        if (parseResult is ParseResult.Success)
            return result;

        string message = parseResult switch
        {
            ParseResult.CommandEmpty => "Input is empty",
            ParseResult.CommandFormat => "Invalid format",
            _ => parseResult.ToString()
        };

        throw new FormatException(message);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public Command Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, out Command result)
        => Parse(input, out result) == ParseResult.Success;

    /// <inheritdoc />
    public bool TryParse(string? input, out Command result)
        => TryParse(input.AsSpan(), out result);

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

    /// <inheritdoc />
    public string ToString(Command command)
    {
        if (command > s_maxNumeric)
            return command.ToString();

        return ((ushort)command).ToString("d3");
    }

    internal enum ParseResult
    {
        Success = 0,
        CommandEmpty,
        CommandFormat,
    }
}
