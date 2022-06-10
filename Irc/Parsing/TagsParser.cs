using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;

namespace Teraa.Irc.Parsing;

/// <summary>
///     <see cref="ITags"/> parser.
/// </summary>
[PublicAPI]
public interface ITagsParser
{
    /// <summary>
    ///     Parses the tags from <paramref name="input"/>.
    ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
    /// </summary>
    /// <param name="input">Tags part of the IRC message.</param>
    ITags Parse(ReadOnlySpan<char> input);

    /// <inheritdoc cref="Parse(System.ReadOnlySpan{char})"/>
    ITags Parse(string input);

    /// <summary>
    ///     Parses the tags from <paramref name="input"/>.
    ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed tags if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out ITags? result);

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, out ITags)"/>
    bool TryParse(string? input, [NotNullWhen(true)] out ITags? result);

    /// <summary>
    ///     Returns the <see cref="string"/> representation of the <see cref="ITags"/>
    /// </summary>
    /// <param name="tags">Tags.</param>
    /// <returns><see cref="string"/> representing the tags.</returns>
    string ToString(ITags tags);
}

/// <inheritdoc />
[PublicAPI]
public class TagsParser : ITagsParser
{
    /// <inheritdoc />
    /// <exception cref="FormatException"><paramref name="input"/> is empty.</exception>
    public ITags Parse(ReadOnlySpan<char> input)
    {
        Result parseResult = Parse(input, out ITags result);
        if (parseResult is Result.Success)
            return result;

        string message = parseResult switch
        {
            Result.Empty => "Input is empty",
            Result.TrailingSemicolon => "Trailing semicolon",
            Result.KeyEmpty => "A key is empty",
            _ => parseResult.ToString()
        };

        throw new FormatException(message);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public ITags Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out ITags? result)
        => Parse(input, out result) == Result.Success;

    /// <inheritdoc />
    public bool TryParse(string? input, [NotNullWhen(true)] out ITags? result)
        => TryParse(input.AsSpan(), out result);

    /// <summary>
    ///     Parses the input as described in the <see href="https://ircv3.net/specs/extensions/message-tags#escaping-values">IRCv3 spec</see>.
    /// </summary>
    /// <param name="input">Escaped value of a message tag.</param>
    /// <returns>Parsed value of the message tag.</returns>
    public static string ParseValue(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty) return "";

        int i = input.IndexOf('\\');
        if (i == -1 || i == input.Length - 1)
            return input.ToString();

        StringBuilder result = new(input.Length - 1);

        do
        {
            char next = input[i + 1];
            char parsed = next switch
            {
                '\\' => '\\',
                ':' => ';',
                's' => ' ',
                'r' => '\r',
                'n' => '\n',
                _ => next
            };

            result.Append(input[..i]);
            result.Append(parsed);

            input = input[(i + 2)..];
            i = input.IndexOf('\\');
        } while (i != -1 && i != input.Length - 1);

        result.Append(input);

        return result.ToString();
    }

    internal static Result Parse(ReadOnlySpan<char> input, out ITags result)
    {
        result = null!;

        if (input.IsEmpty)
            return Result.Empty;

        Dictionary<string, string> tags = new();

        int i;
        ReadOnlySpan<char> tag;
        do
        {
            i = input.IndexOf(';');
            if (i == -1)
            {
                tag = input;
                input = default;
            }
            else
            {
                tag = input[..i];
                input = input[(i + 1)..];

                if (input.IsEmpty)
                    return Result.TrailingSemicolon;
            }

            ReadOnlySpan<char> key, value;
            i = tag.IndexOf('=');
            if (i == -1)
            {
                key = tag;
                value = default;
            }
            else
            {
                key = tag[..i];
                value = ParseValue(tag[(i + 1)..]);
            }

            if (key.IsEmpty)
                return Result.KeyEmpty;

            tags[key.ToString()] = value.ToString();
        } while (!input.IsEmpty);

        result = new Tags(tags);

        return Result.Success;
    }

    /// <summary>
    ///     Escapes input as described in the <see href="https://ircv3.net/specs/extensions/message-tags#escaping-values">IRCv3 spec</see>.
    /// </summary>
    /// <param name="input">Parsed value of a message tag.</param>
    /// <returns>Escaped value of the message tag.</returns>
    public static string EscapeValue(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty) return "";

        StringBuilder result = new();
        int i = 0;
        for (int j = 0; j < input.Length; j++)
        {
            string? escaped = input[j] switch
            {
                '\\' => @"\\",
                ';' => @"\:",
                ' ' => @"\s",
                '\r' => @"\r",
                '\n' => @"\n",
                _ => null
            };

            if (escaped is not null)
            {
                result
                    .Append(input[i..j])
                    .Append(escaped);

                i = j + 1;
            }
        }

        result.Append(input[i..]);

        return result.ToString();
    }

    /// <inheritdoc />
    public string ToString(ITags tags)
    {
        if (tags.Count == 0) return "";

        StringBuilder result = new();
        foreach ((string key, string value) in tags)
        {
            result.Append(key);

            if (value is {Length: > 0})
                result
                    .Append('=')
                    .Append(EscapeValue(value));

            result.Append(';');
        }

        // Remove trailing semicolon
        result.Remove(result.Length - 1, 1);

        return result.ToString();
    }

    internal enum Result
    {
        Success = 0,
        Empty,
        TrailingSemicolon,
        KeyEmpty,
    }
}
