using System.Text;
using JetBrains.Annotations;
using Teraa.Irc.Parsing;

namespace Teraa.Irc;

/// <summary>
///     Type representing an IRC message.
///     Message format is defined in <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see>.
/// </summary>
[PublicAPI]
public interface IMessage
{
    /// <summary>
    ///     IRC command.
    /// </summary>
    Command Command { get; }
    /// <summary>
    ///     Message tags.
    /// </summary>
    ITags? Tags { get; }
    /// <summary>
    ///     Message prefix.
    /// </summary>
    IPrefix? Prefix { get; }
    /// <summary>
    ///     Message argument.
    /// </summary>
    string? Arg { get; }
    /// <summary>
    ///     Message content.
    /// </summary>
    IContent? Content { get; }
}

/// <inheritdoc />
[PublicAPI]
public record Message(
    Command Command,
    ITags? Tags = null,
    IPrefix? Prefix = null,
    string? Arg = null,
    IContent? Content = null
) : IMessage
{

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder result = new();

        if (Tags is { Count: > 0 })
            result
                .Append('@')
                .Append(Tags)
                .Append(' ');

        if (Prefix is not null)
            result
                .Append(':')
                .Append(Prefix)
                .Append(' ');

        result.Append(CommandParser.ToString(Command));

        if (Arg is not null)
            result
                .Append(' ')
                .Append(Arg);

        if (Content is not null)
            result
                .Append(" :")
                .Append(Content);


        return result.ToString();
    }
}
