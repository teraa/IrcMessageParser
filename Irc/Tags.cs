using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Teraa.Irc;

/// <summary>
///     Class representing a collection of key/value pairs of <see cref="Message"/> tags.
/// </summary>
public class Tags : IReadOnlyDictionary<string, string>
{
    private readonly IReadOnlyDictionary<string, string> _tags;

    /// <summary>
    ///     Initializes a new <see cref="Tags"/> instance with the provided <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="tags">A <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing tag key/value pairs.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tags"/> is null.</exception>
    public Tags(IReadOnlyDictionary<string, string> tags)
        => _tags = tags ?? throw new ArgumentNullException(nameof(tags));

    /// <inheritdoc/>
    public static implicit operator Tags(Dictionary<string, string> tags)
        => new(tags);

    /// <inheritdoc cref="Parse(ReadOnlySpan{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public static Tags Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <summary>
    ///     Parses the tags from <paramref name="input"/>.
    ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
    /// </summary>
    /// <param name="input">Tags part of the IRC message.</param>
    /// <exception cref="FormatException"><paramref name="input"/> is empty.</exception>
    public static Tags Parse(ReadOnlySpan<char> input)
    {
        ParseResult parseResult = Parse(input, out Tags result);
        if (parseResult is ParseResult.Success)
            return result;

        throw new FormatException(parseResult.ParseResultToString());
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, out Tags)"/>
    public static bool TryParse(string? input, out Tags result)
        => TryParse(input.AsSpan(), out result);

    /// <summary>
    ///     Parses the tags from <paramref name="input"/>.
    ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed tags if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out Tags result)
        => Parse(input, out result) == ParseResult.Success;

    /// <summary>
    ///     Parses the input as described in the <see href="https://ircv3.net/specs/extensions/message-tags#escaping-values">IRCv3 spec</see>.
    /// </summary>
    /// <param name="input">Escaped value of a message tag.</param>
    /// <returns>Parsed value of the message tag.</returns>
    public static string ParseValue(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty) return "";

        StringBuilder result = new();
        int i = 0;
        for (int j = 0; j < input.Length - 1; j++)
        {
            if (input[j] == '\\')
            {
                result.Append(input[i..j]);

                j++;
                char next = input[j];
                char parsed = next switch
                {
                    '\\' => '\\',
                    ':' => ';',
                    's' => ' ',
                    'r' => '\r',
                    'n' => '\n',
                    _ => next
                };

                result.Append(parsed);
                i = j + 1;
            }
        }

        result.Append(input[i..]);

        return result.ToString();
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

    /// <inheritdoc/>
    public string this[string key]
        => _tags[key];

    /// <inheritdoc/>
    public IEnumerable<string> Keys
        => _tags.Keys;

    /// <inheritdoc/>
    public IEnumerable<string> Values
        => _tags.Values;

    /// <inheritdoc/>
    public int Count
        => _tags.Count;

    /// <inheritdoc/>
    public bool ContainsKey(string key)
        => _tags.ContainsKey(key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        => _tags.GetEnumerator();

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => _tags.TryGetValue(key, out value);

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder result = new();

        if (_tags is { Count: > 0 })
        {
            foreach ((string key, string value) in _tags)
            {
                result.Append(key);

                if (value is { Length: > 0 })
                    result
                        .Append('=')
                        .Append(EscapeValue(value));

                result.Append(';');
            }

            // Remove trailing semicolon
            result.Remove(result.Length - 1, 1);
        }

        return result.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
        => _tags.GetEnumerator();

    internal static ParseResult Parse(ReadOnlySpan<char> input, out Tags result)
    {
        result = null!;

        if (input.IsEmpty)
            return ParseResult.TagsEmpty;

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
                    return ParseResult.TagsTrailingSemicolon;
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
                return ParseResult.TagsKeyEmpty;

            tags[key.ToString()] = value.ToString();

        } while (!input.IsEmpty);

        result = new Tags(tags);

        return ParseResult.Success;
    }
}
