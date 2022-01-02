using System;

namespace Teraa.Irc;

/// <summary>
///     Record representing content of a <see cref="Message"/>.
///     See <see href="https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html"/> for details.
/// </summary>
public record Content
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
    ///     Initializes a new <see cref="Content"/> instance with provided values.
    /// </summary>
    /// <param name="text">Content text.</param>
    /// <param name="ctcp">Client-to-Client Protocol command.</param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
    public Content(string text, string? ctcp = null)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Ctcp = ctcp;
    }

    /// <inheritdoc/>
    public static implicit operator string(Content content)
    {
        return content.Ctcp is null
            ? content.Text
            : $"{s_ctcpDelimiter}{content.Ctcp} {content.Text}{s_ctcpDelimiter}";
    }

    /// <inheritdoc/>
    public static explicit operator Content(string s)
        => Parse(s);

    /// <inheritdoc cref="Parse(ReadOnlySpan{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public static Content Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Content"/>.
    ///     See <see href="https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html"/> for details.
    /// </summary>
    /// <param name="input">Content.</param>
    /// <returns><see cref="Content"/> instance parsed from <paramref name="input"/>.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public static Content Parse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            throw new FormatException("Input is empty");

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

        string text = input.ToString();

        return new Content(text, ctcp);
    }

    /// <inheritdoc/>
    public override string ToString() => this;
}
