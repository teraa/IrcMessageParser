using System;
using JetBrains.Annotations;

namespace Teraa.Irc.Parsing;

/// <inheritdoc />
[PublicAPI]
public class FastCommandParser : ICommandParser
{
    private const Command s_maxNumeric = (Command) 999;

    /// <inheritdoc />
    public Command Parse(ReadOnlySpan<char> input)
        => Parse(input.ToString());

    /// <inheritdoc />
    public Command Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        Result parseResult = Parse(input, out Command result);
        if (parseResult is Result.Success)
            return result;

        string message = parseResult switch
        {
            Result.Empty => "Input is empty",
            Result.InvalidFormat => "Invalid format",
            Result.InvalidNumeric => $"Invalid numeric format",
            _ => parseResult.ToString()
        };

        throw new FormatException(message);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, out Command result)
        => TryParse(input.ToString(), out result);

    /// <inheritdoc />
    public bool TryParse(string? input, out Command result)
    {
        if (input is null)
        {
            result = 0;
            return false;
        }

        return Parse(input, out result) == Result.Success;
    }

    internal static Result Parse(string input, out Command result)
    {
        if (input.Length == 0)
        {
            result = 0;
            return Result.Empty;
        }

        if (!CommandExtensions.TryParse(input, out result))
            return Result.InvalidFormat;

        if (input[0] is < '0' or > '9')
            return Result.Success;

        if (input.Length == 3 && result <= s_maxNumeric)
            return Result.Success;

        result = 0;
        return Result.InvalidNumeric;
    }

    /// <inheritdoc />
    public string ToString(Command command)
    {
        return command > s_maxNumeric
            ? command.ToStringFast()
            : ((ushort) command).ToString("d3");
    }

    internal enum Result
    {
        Success = 0,
        Empty,
        InvalidFormat,
        InvalidNumeric,
    }
}
