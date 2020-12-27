using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace IrcMessageParser
{
    public class MessageTags : IReadOnlyDictionary<string, string>
    {
        private readonly IReadOnlyDictionary<string, string> _tags;

        public MessageTags(IReadOnlyDictionary<string, string> tags)
            => _tags = tags ?? throw new ArgumentNullException(nameof(tags));

        public static implicit operator MessageTags(Dictionary<string, string> tags)
            => new(tags);

        /// <summary>
        ///     Parses the tags from <paramref name="input"/>.
        ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
        /// </summary>
        /// <param name="input">Tags part of the IRC message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
        public static MessageTags Parse(ReadOnlySpan<char> input)
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
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        public static string ParseValue(ReadOnlySpan<char> input)
        {
            var result = new StringBuilder();
            int lastSplit = 0;
            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i] == '\\')
                {
                    result.Append(input[lastSplit..i]);

                    i++;
                    var next = input[i];
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
                    lastSplit = i + 1;
                }
            }

            result.Append(input[lastSplit..]);

            return result.ToString();
        }

        /// <summary>
        ///     Escapes input as described in the <see href="https://ircv3.net/specs/extensions/message-tags#escaping-values">IRCv3 spec</see>.
        /// </summary>
        /// <param name="input">Parsed value of a message tag.</param>
        /// <returns>Escaped value of the message tag.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        public static string EscapeValue(ReadOnlySpan<char> input)
        {
            var result = new StringBuilder();
            int lastSplit = 0;
            for (int i = 0; i < input.Length; i++)
            {
                var escaped = input[i] switch
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
                        .Append(input[lastSplit..i])
                        .Append(escaped);

                    lastSplit = i + 1;
                }
            }

            result.Append(input[lastSplit..]);

            return result.ToString();
        }

        public string this[string key]
            => _tags[key];

        public IEnumerable<string> Keys
            => _tags.Keys;

        public IEnumerable<string> Values
            => _tags.Values;

        public int Count
            => _tags.Count;

        public bool ContainsKey(string key)
            => _tags.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => _tags.GetEnumerator();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
            => _tags.TryGetValue(key, out value);

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
}
