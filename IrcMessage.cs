using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Twitch.Irc
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
        private const string ActionStart = "\u0001ACTION ";
        private const char ActionEnd = '\u0001';

        public IReadOnlyDictionary<string, string>? Tags { get; init; }
        public string? Hostmask { get; init; }
        public IrcCommand Command { get; init; }
        public string? Arg { get; init; }
        public string? Content { get; init; }
        public bool? IsAction { get; init; }

        internal IrcMessage() { }

        /// <summary>
        ///     Parses the <see cref="IrcMessage"/> from a raw IRC message (<paramref name="input"/>).
        /// </summary>
        /// <param name="input">Raw IRC message.</param>
        /// <returns><see cref="IrcMessage"/> instance parsed from <paramref name="input"/>.</returns>
        /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
        internal static IrcMessage Parse(ReadOnlySpan<char> input)
        {
            IReadOnlyDictionary<string, string>? tags;
            IrcCommand command;
            string? hostmask, arg, content;
            bool? isAction;

            int i, startIndex = 0;

            // Tags
            if (input[startIndex] == '@')
            {
                startIndex++;
                i = input[startIndex..].IndexOf(' ');
                i += i != -1 ? startIndex : throw new FormatException();

                tags = ParseTags(input[startIndex..i]);
                startIndex = i + 1;
            }
            else
            {
                tags = null;
            }

            // Hostmask
            if (input[startIndex] == ':')
            {
                startIndex++;
                i = input[startIndex..].IndexOf(' ');
                i += i != -1 ? startIndex : throw new FormatException();

                hostmask = input[startIndex..i].ToString();
                startIndex = i + 1;
            }
            else
            {
                hostmask = null;
            }

            // Command
            i = input[startIndex..].IndexOf(' ');
            if (i != -1)
            {
                i += startIndex;
                command = ParseCommand(input[startIndex..i]);
                startIndex = i + 1;

                // Arg & Content
                const string contentStart = " :";
                i = input[startIndex..].IndexOf(contentStart, StringComparison.Ordinal);
                if (i != -1)
                {
                    i += startIndex;
                    arg = input[startIndex..i].ToString();
                    startIndex = i + contentStart.Length;
                    var contentSpan = input[startIndex..];

                    isAction = contentSpan.StartsWith(ActionStart, StringComparison.Ordinal);
                    if (isAction == true)
                        contentSpan = contentSpan[ActionStart.Length..].TrimEnd(ActionEnd);

                    content = contentSpan.ToString();
                }
                else
                {
                    arg = input[startIndex..].ToString();
                    content = null;
                    isAction = null;
                }
            }
            else
            {
                command = ParseCommand(input[startIndex..]);
                arg = null;
                content = null;
                isAction = null;
            }

            return new IrcMessage
            {
                Arg = arg,
                Command = command,
                Content = content,
                Hostmask = hostmask,
                IsAction = isAction,
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
        internal static IrcCommand ParseCommand(ReadOnlySpan<char> input)
        {
            var inputStr = input.ToString();
            if (Enum.TryParse(inputStr, out IrcCommand command)
                && (Enum.IsDefined(typeof(IrcCommand), command) || inputStr.Length == 3))
                return command;

            throw new FormatException($"Unknown command format: {inputStr}");
        }

        /// <summary>
        ///     Parses the tags from <paramref name="input"/> into <see cref="ReadOnlyDictionary{TKey, TValue}"/>.
        ///     See <see href="https://ircv3.net/specs/extensions/message-tags#format">IRCv3 spec</see> for details.
        /// </summary>
        /// <param name="input">Tags part of the IRC message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
        internal static ReadOnlyDictionary<string, string> ParseTags(ReadOnlySpan<char> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.IsEmpty)
                throw new ArgumentException("Argument cannot be empty", nameof(input));

            var tags = new Dictionary<string, string>();

            int i = -1, startIndex;
            do
            {
                startIndex = i + 1;
                i = input[startIndex..].IndexOf(';');
                var endIndex = i == -1 ? ^0 : i += startIndex;

                var tag = input[startIndex..endIndex];

                string key, value;
                var splitIndex = tag.IndexOf('=');
                if (splitIndex == -1)
                {
                    key = tag.ToString();
                    value = "";
                }
                else
                {
                    key = tag[..splitIndex].ToString();
                    value = ParseTagValue(tag[(splitIndex + 1)..]);
                }

                tags[key] = value;

            } while (i != -1);

            return new ReadOnlyDictionary<string, string>(tags);
        }

        /// <summary>
        ///     Parses the input as described in the <see href="https://ircv3.net/specs/extensions/message-tags#escaping-values">IRCv3 spec</see>.
        /// </summary>
        /// <param name="input">Escaped value of a message tag.</param>
        /// <returns>Parsed value of the message tag.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        internal static string ParseTagValue(ReadOnlySpan<char> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

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
        internal static string EscapeTagValue(ReadOnlySpan<char> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

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

                if (escaped != null)
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

            if (Tags != null)
            {
                result.Append('@');

                // There is always at least one element
                foreach (var (key, value) in Tags)
                    result
                        .Append(key)
                        .Append('=')
                        .Append(EscapeTagValue(value))
                        .Append(';');

                // Remove trailing semicolon
                result.Remove(result.Length - 1, 1);

                result.Append(' ');
            }

            if (Hostmask != null)
                result
                    .Append(':')
                    .Append(Hostmask)
                    .Append(' ');

            if (Enum.IsDefined(typeof(IrcCommand), Command))
                result.Append(Command);
            else
                result.Append(((ushort)Command).ToString("d3"));

            if (Arg != null)
                result
                    .Append(' ')
                    .Append(Arg);

            if (Content != null)
            {
                result.Append(" :");

                if (IsAction == true)
                    result
                        .Append(ActionStart)
                        .Append(Content)
                        .Append(ActionEnd);
                else
                    result.Append(Content);
            }


            return result.ToString();
        }

        private string DebuggerDisplay => ToString();
    }
}
