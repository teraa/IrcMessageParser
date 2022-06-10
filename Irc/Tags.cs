using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;

namespace Teraa.Irc;

/// <summary>
///     Type representing a collection of key/value pairs of <see cref="IMessage"/> tags.
/// </summary>
[PublicAPI]
public interface ITags : IReadOnlyDictionary<string, string> { }

/// <inheritdoc />
[PublicAPI]
public class Tags : ITags
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
    public override string ToString()
    {
        StringBuilder result = new();

        if (_tags is {Count: > 0})
        {
            foreach ((string key, string value) in _tags)
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
        }

        return result.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
        => _tags.GetEnumerator();
}
