using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Teraa.Irc.Parsing;

/// <summary>
///     <see cref="IContent"/> parser.
/// </summary>
[PublicAPI]
public interface IContentParser
{
    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="IContent"/>.
    ///     See <see href="https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html"/> for details.
    /// </summary>
    /// <param name="input">Content.</param>
    /// <returns><see cref="IContent"/> instance parsed from <paramref name="input"/>.</returns>
    IContent Parse(ReadOnlySpan<char> input);

    /// <inheritdoc cref="Parse(System.ReadOnlySpan{char})"/>
    IContent Parse(string input);

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="IContent"/>.
    ///     See <see href="https://tools.ietf.org/id/draft-oakley-irc-ctcp-01.html"/> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed content if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out IContent? result);

    /// <inheritdoc cref="TryParse(System.ReadOnlySpan{char},out Teraa.Irc.IContent)"/>
    bool TryParse(string? input, [NotNullWhen(true)] out IContent? result);

    /// <summary>
    ///     Returns the <see cref="string"/> representation of the <see cref="IContent"/>
    /// </summary>
    /// <param name="content">Content.</param>
    /// <returns><see cref="string"/> representing the content.</returns>
    string ToString(IContent content);
}

/// <inheritdoc />
[PublicAPI]
public class ContentParser : IContentParser
{
    private const char s_ctcpDelimiter = '\u0001';

    /// <inheritdoc />
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public IContent Parse(ReadOnlySpan<char> input)
    {
        Result parseResult = Parse(input, out IContent result);
        if (parseResult is Result.Success)
            return result;

        string message = parseResult switch
        {
            Result.Empty => "Input is empty",
            Result.MissingCtcpEnding => "Missing content CTCP ending",
            _ => parseResult.ToString()
        };

        throw new FormatException(message);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public IContent Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out IContent? result)
        => Parse(input, out result) == Result.Success;

    /// <inheritdoc />
    public bool TryParse(string? input, [NotNullWhen(true)] out IContent? result)
        => TryParse(input.AsSpan(), out result);

    internal static Result Parse(ReadOnlySpan<char> input, out IContent result)
    {
        result = null!;

        if (input.IsEmpty)
            return Result.Empty;

        string? ctcp;
        if (input[0] == s_ctcpDelimiter)
        {
            input = input[1..];
            if (input[^1] == s_ctcpDelimiter)
                input = input[..^1];

            int i = input.IndexOf(' ');

            if (i == -1)
                return Result.MissingCtcpEnding;

            ctcp = input[..i].ToString();
            input = input[(i + 1)..];
        }
        else
        {
            ctcp = null;
        }

        string text = input.ToString();

        result = new Content(text, ctcp);

        return Result.Success;
    }

    /// <inheritdoc/>
    public string ToString(IContent content)
        => content.Ctcp is null
            ? content.Text
            : $"{s_ctcpDelimiter}{content.Ctcp} {content.Text}{s_ctcpDelimiter}";

    internal enum Result
    {
        Success = 0,
        Empty,
        MissingCtcpEnding,
    }
}
