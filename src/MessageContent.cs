using System;

namespace Teraa.IrcMessageParser;

/// <summary>
///     Record representing content of a <see cref="IrcMessage"/>.
///     See <see href="https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html"/> for details.
/// </summary>
public record MessageContent
{
    private const char s_ctcpDelimiter = '\u0001';

    /// <summary>
    ///     Content text.
    /// </summary>
    public string Text { get; }
    /// <summary>
    ///     Client-to-Client Protocol command.
    /// </summary>
    public string? Ctcp { get; }

    /// <summary>
    ///     Initializes a new <see cref="MessageContent"/> instance with provided values.
    /// </summary>
    /// <param name="text">Content text.</param>
    /// <param name="ctcp">Client-to-Client Protocol command.</param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
    public MessageContent(string text, string? ctcp = null)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Ctcp = ctcp;
    }

    /// <inheritdoc/>
    public static implicit operator string(MessageContent content)
    {
        return content.Ctcp is null
            ? content.Text
            : $"{s_ctcpDelimiter}{content.Ctcp} {content.Text}{s_ctcpDelimiter}";
    }

    /// <inheritdoc/>
    public static explicit operator MessageContent(string s)
        => Parse(s);

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="MessageContent"/>.
    ///     See <see href="https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html"/> for details.
    /// </summary>
    /// <param name="input">Content.</param>
    /// <returns><see cref="MessageContent"/> instance parsed from <paramref name="input"/>.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public static MessageContent Parse(ReadOnlySpan<char> input)
    {
        string? ctcp;
        if (input[0] == s_ctcpDelimiter)
        {
            input = input[1..];
            if (input[^1] == s_ctcpDelimiter)
                input = input[..^1];

            int i = input.IndexOf(' ');

            if (i == -1)
                throw new FormatException("Missing CTCP ending");

            ctcp = input[..i].ToString();
            input = input[(i + 1)..];
        }
        else
        {
            ctcp = null;
        }

        var text = input.ToString();

        return new MessageContent(text, ctcp);
    }

    /// <inheritdoc/>
    public override string ToString() => this;
}
