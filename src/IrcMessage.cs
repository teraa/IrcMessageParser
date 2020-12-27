using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace IrcMessageParser
{
    public enum IrcCommand : ushort
    {
        CAP = 1000,
        CLEARCHAT,
        CLEARMSG,
        GLOBALUSERSTATE,
        HOSTTARGET,
        JOIN,
        MODE,
        NICK,
        NOTICE,
        PART,
        PASS,
        PING,
        PONG,
        PRIVMSG,
        RECONNECT,
        ROOMSTATE,
        USERNOTICE,
        USERSTATE,
        WHISPER
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class IrcMessage
    {
        public IReadOnlyDictionary<string, string>? Tags { get; init; }
        public string? Hostmask { get; init; }
        public IrcCommand Command { get; init; }
        public string? Arg { get; init; }
        public MessageContent? Content { get; init; }

        public IrcMessage() { }

        /// <summary>
        ///     Parses the <see cref="IrcMessage"/> from a raw IRC message (<paramref name="input"/>).
        /// </summary>
        /// <param name="input">Raw IRC message.</param>
        /// <returns><see cref="IrcMessage"/> instance parsed from <paramref name="input"/>.</returns>
        /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
        public static IrcMessage Parse(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
                throw new ArgumentException("Argument cannot be empty", nameof(input));

            IReadOnlyDictionary<string, string>? tags;
            string? hostmask;
            IrcCommand command;
            string? arg;
            MessageContent? content;

            int i;

            // Tags
            if (input[0] == '@')
            {
                input = input[1..];
                i = input.IndexOf(' ');

                if (i == -1)
                    throw new FormatException("Missing tags ending");

                tags = ParseTags(input[..i]);
                input = input[(i + 1)..];
            }
            else
            {
                tags = null;
            }

            // Hostmask
            if (input[0] == ':')
            {
                input = input[1..];
                i = input.IndexOf(' ');

                if (i == -1)
                    throw new FormatException("Missing hostmask ending");

                hostmask = input[..i].ToString();
                input = input[(i + 1)..];
            }
            else
            {
                hostmask = null;
            }

            // Command
            i = input.IndexOf(' ');
            if (i != -1)
            {
                command = ParseCommand(input[..i]);
                input = input[(i + 1)..];

                // No Arg
                if (input[0] == ':')
                {
                    input = input[1..];
                    arg = null;
                }
                else
                {
                    const string contentStart = " :";
                    i = input.IndexOf(contentStart, StringComparison.Ordinal);

                    // No content
                    if (i == -1)
                    {
                        arg = input.ToString();
                        input = default;
                    }
                    else
                    {
                        arg = input[..i].ToString();
                        input = input[(i + contentStart.Length)..];
                    }
                }

                content = input.IsEmpty
                    ? null
                    : MessageContent.Parse(input);
            }
            else
            {
                command = ParseCommand(input);
                arg = null;
                content = null;
            }

            return new IrcMessage
            {
                Arg = arg,
                Command = command,
                Content = content,
                Hostmask = hostmask,
                Tags = tags
            };
        }

        /// <summary>
        ///     Parses the <see cref="IrcCommand"/> from <paramref name="input"/>.
        ///     See <see href="https://tools.ietf.org/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
        /// </summary>
        /// <param name="input">Input to parse.</param>
        /// <returns><see cref="IrcCommand"/>.</returns>
        /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
        public static IrcCommand ParseCommand(ReadOnlySpan<char> input)
        {
            var inputStr = input.ToString();
            if (Enum.TryParse(inputStr, out IrcCommand command)
                && (Enum.IsDefined(command) || inputStr.Length == 3))
                return command;

            throw new FormatException($"Invalid command format: {inputStr}");
        }

        /// <summary>
        ///     Parses the tags from <paramref name="input"/> into <see cref="Dictionary{TKey, TValue}"/>.
        ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
        /// </summary>
        /// <param name="input">Tags part of the IRC message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
        public static Dictionary<string, string> ParseTags(ReadOnlySpan<char> input)
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
                    value = ParseTagValue(tag[(i + 1)..]);
                }

                tags[key] = value;

            } while (!input.IsEmpty);

            return tags;
        }

        /// <summary>
        ///     Parses the input as described in the <see href="https://ircv3.net/specs/extensions/message-tags#escaping-values">IRCv3 spec</see>.
        /// </summary>
        /// <param name="input">Escaped value of a message tag.</param>
        /// <returns>Parsed value of the message tag.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        public static string ParseTagValue(ReadOnlySpan<char> input)
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
        public static string EscapeTagValue(ReadOnlySpan<char> input)
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

        public override string ToString()
        {
            var result = new StringBuilder();

            if (Tags is { Count: > 0 })
            {
                result.Append('@');

                foreach (var (key, value) in Tags)
                {
                    result.Append(key);

                    if (value is { Length: > 0 })
                        result
                            .Append('=')
                            .Append(EscapeTagValue(value));
                    
                    result.Append(';');
                }

                // Remove trailing semicolon
                result.Remove(result.Length - 1, 1);

                result.Append(' ');
            }

            if (Hostmask is not null)
                result
                    .Append(':')
                    .Append(Hostmask)
                    .Append(' ');

            if (Enum.IsDefined(Command))
                result.Append(Enum.GetName(Command));
            else
                result.Append(((ushort)Command).ToString("d3"));

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

        private string DebuggerDisplay => ToString();
    }
}
