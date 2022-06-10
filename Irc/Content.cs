using JetBrains.Annotations;

namespace Teraa.Irc;

/// <summary>
///     Type representing content of a <see cref="IMessage"/>.
///     See <see href="https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html"/> for details.
/// </summary>
public interface IContent
{
    /// <summary>
    ///     Content text.
    /// </summary>
    string Text { get; }
    /// <summary>
    ///     Client-to-Client Protocol command.
    /// </summary>
    string? Ctcp { get; }
}

/// <inheritdoc />
[PublicAPI]
public record Content(
    string Text,
    string? Ctcp = null
): IContent
{
    internal const char CtcpDelimiter = '\u0001';

    /// <inheritdoc/>
    public override string ToString()
        => Ctcp is null
            ? Text
            : $"{CtcpDelimiter}{Ctcp} {Text}{CtcpDelimiter}";
}
