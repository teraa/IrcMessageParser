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

    /// <summary>
    ///     Parses the tags from <paramref name="input"/>.
    ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
    /// </summary>
    /// <param name="input">Tags part of the IRC message.</param>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
    public static Tags Parse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            throw new ArgumentException("Argument cannot be empty", nameof(input));

        var tags = new Dictionary<string, string>();

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
            }

            string key, value;
            i = tag.IndexOf('=');
            if (i == -1)
            {
                key = tag.ToString();
                value = "";
            }
            else
            {
                key = tag[..i].ToString();
                value = ParseValue(tag[(i + 1)..]);
            }

            tags[key] = value;

        } while (!input.IsEmpty);

        return new(tags);
    }

    /// <summary>
    ///     Parses the input as described in the <see href="https://ircv3.net/specs/extensions/message-tags#escaping-values">IRCv3 spec</see>.
    /// </summary>
    /// <param name="input">Escaped value of a message tag.</param>
    /// <returns>Parsed value of the message tag.</returns>
    public static string ParseValue(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty) return "";

        var result = new StringBuilder();
        int i = 0;
        for (int j = 0; j < input.Length - 1; j++)
        {
            if (input[j] == '\\')
            {
                result.Append(input[i..j]);

                j++;
                var next = input[j];
                var parsed = next switch
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

        var result = new StringBuilder();
        int i = 0;
        for (int j = 0; j < input.Length; j++)
        {
            var escaped = input[j] switch
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
        var result = new StringBuilder();

        if (_tags is { Count: > 0 })
        {
            foreach (var (key, value) in _tags)
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
}
