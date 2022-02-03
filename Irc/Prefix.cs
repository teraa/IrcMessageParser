using System;
using System.Runtime.CompilerServices;

namespace Teraa.Irc;

/// <summary>
///     Record representing message prefix of a <see cref="Message"/>.
///     Prefix contains information about the server or user sending the message.
///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
/// </summary>
public record Prefix
{
    /// <summary>
    ///     Server name or user nick.
    /// </summary>
    public string Name { get; }
    /// <summary>
    ///     User.
    /// </summary>
    public string? User { get; }
    /// <summary>
    ///     Host.
    /// </summary>
    public string? Host { get; }

    /// <summary>
    ///     Initializes a new <see cref="Prefix"/> instance with provided values.
    /// </summary>
    /// <param name="name">Server name or user nick.</param>
    /// <param name="user">User.</param>
    /// <param name="host">Host.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    public Prefix(string name, string? user, string? host)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        User = user;
        Host = host;
    }

    /// <inheritdoc cref="Parse(ReadOnlySpan{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public static Prefix Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Prefix"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Content.</param>
    /// <returns><see cref="Prefix"/> instance parsed from <paramref name="input"/>.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public static Prefix Parse(ReadOnlySpan<char> input)
    {
        FailResult status = Parse(input, out Prefix result);
        if (status is FailResult.None)
            return result;

        throw new FormatException(status.ReasonToString());
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, out Prefix)"/>
    public static bool TryParse(string? input, out Prefix result)
        => TryParse(input.AsSpan(), out result);

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Prefix"/>.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed prefix if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out Prefix result)
        => Parse(input, out result) == FailResult.None;

    /// <inheritdoc/>
    public override string ToString()
    {
        return (User, Host) switch
        {
            (not null, not null) => $"{Name}!{User}@{Host}",
            (not null, null) => $"{Name}!{User}",
            (null, not null) => $"{Name}@{Host}",
            (null, null) => Name,
        };
    }

    internal static FailResult Parse(ReadOnlySpan<char> input, out Prefix result)
    {
        result = null!;

        if (input.IsEmpty)
            return FailResult.PrefixEmpty;

        string name;
        string? user, host;

        int i = input.IndexOf('@');

        if (i != -1)
        {
            var hostSpan = input[(i + 1)..];
            if (hostSpan.IsEmpty)
                return FailResult.PrefixEmptyHost;

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
            var userSpan = input[(i + 1)..];
            if (userSpan.IsEmpty)
                return FailResult.PrefixEmptyUser;

            user = userSpan.ToString();
            input = input[..i];
        }
        else
        {
            user = null;
        }

        if (input.IsEmpty)
            return FailResult.PrefixEmptyName;

        name = input.ToString();

        result = new Prefix(name, user, host);

        return FailResult.None;
    }
}
