using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Teraa.Irc.Parsing;

/// <summary>
///     <see cref="IPrefix"/> parser.
/// </summary>
[PublicAPI]
public interface IPrefixParser
{
    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Prefix"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Content.</param>
    /// <returns><see cref="Prefix"/> instance parsed from <paramref name="input"/>.</returns>
    IPrefix Parse(ReadOnlySpan<char> input);

    /// <inheritdoc cref="Parse(System.ReadOnlySpan{char})"/>
    IPrefix Parse(string input);

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Prefix"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed prefix if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out IPrefix? result);

    /// <inheritdoc cref="TryParse(System.ReadOnlySpan{char},out Teraa.Irc.IPrefix)"/>
    bool TryParse(string? input, [NotNullWhen(true)] out IPrefix? result);
}

/// <inheritdoc />
[PublicAPI]
public class PrefixParser : IPrefixParser
{
    /// <inheritdoc />
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public IPrefix Parse(ReadOnlySpan<char> input)
    {
        Result parseResult = Parse(input, out IPrefix result);
        if (parseResult is Result.Success)
            return result;

        string message = parseResult switch
        {
            Result.Empty => "Input is empty",
            Result.EmptyHost => "Host is empty",
            Result.EmptyUser => "User is empty",
            Result.EmptyName => "Name is empty",
            _ => parseResult.ToString()
        };

        throw new FormatException(message);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public IPrefix Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out IPrefix? result)
        => Parse(input, out result) == Result.Success;

    /// <inheritdoc />
    public bool TryParse(string? input, [NotNullWhen(true)] out IPrefix? result)
        => TryParse(input.AsSpan(), out result);

    internal static Result Parse(ReadOnlySpan<char> input, out IPrefix result)
    {
        result = null!;

        if (input.IsEmpty)
            return Result.Empty;

        string name;
        string? user, host;

        int i = input.IndexOf('@');

        if (i != -1)
        {
            ReadOnlySpan<char> hostSpan = input[(i + 1)..];

            if (hostSpan.IsEmpty)
                return Result.EmptyHost;

            host = hostSpan.ToString();
            input = input[..i];
        }
        else
        {
            host = null;
        }

        i = input.IndexOf('!');

        if (i != -1)
        {
            ReadOnlySpan<char> userSpan = input[(i + 1)..];

            if (userSpan.IsEmpty)
                return Result.EmptyUser;

            user = userSpan.ToString();
            input = input[..i];
        }
        else
        {
            user = null;
        }

        if (input.IsEmpty)
            return Result.EmptyName;

        name = input.ToString();

        result = new Prefix(name, user, host);

        return Result.Success;
    }

    internal enum Result
    {
        Success = 0,
        Empty,
        EmptyHost,
        EmptyUser,
        EmptyName,
    }
}
