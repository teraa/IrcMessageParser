using System;
using System.Runtime.CompilerServices;

namespace Teraa.Irc;

/// <summary>
///     Class for parsing <see cref="Command"/> values.
/// </summary>
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
        ParseStatus status = Parse(input, out Command result);
        if (status is ParseStatus.Success)
            return result;

        throw new FormatException(ParseStatusToString(status));
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
        => Parse(input, out result) == ParseStatus.Success;

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

    internal static ParseStatus Parse(ReadOnlySpan<char> input, out Command result)
    {
        result = 0;

        if (input.IsEmpty)
            return ParseStatus.FailEmpty;

        if (input.Length == 3 && ushort.TryParse(input, out var numeric))
            result = (Command)numeric;
        else if (Enum.TryParse<Command>(input, true, out var command)
            && command is > s_maxNumeric
            && (input[0] is (< '0' or > '9')))
                result = command;
        else
            return ParseStatus.FailFormat;

        return ParseStatus.Success;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string? ParseStatusToString(ParseStatus status)
    {
        return status switch
        {
            ParseStatus.FailEmpty => "Input is empty",
            ParseStatus.FailFormat => "Invalid format",
            _ => null,
        };
    }

    internal enum ParseStatus
    {
        Success,
        FailEmpty,
        FailFormat,
    }
}
