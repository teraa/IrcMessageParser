using System;
using JetBrains.Annotations;

namespace Teraa.Irc;

/// <summary>
///     Type representing message prefix of a <see cref="IMessage"/>.
///     Prefix contains information about the server or user sending the message.
///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
/// </summary>
[PublicAPI]
public interface IPrefix
{
    /// <summary>
    ///     Server name or user nick.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     User.
    /// </summary>
    string? User { get; }

    /// <summary>
    ///     Host.
    /// </summary>
    string? Host { get; }
}

/// <inheritdoc />
[PublicAPI]
public record Prefix(
    string Name,
    string? User = null,
    string? Host = null
) : IPrefix;
