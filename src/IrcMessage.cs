using System;
using System.Diagnostics;
using System.Text;

namespace IrcMessageParser
{
    /// <summary>
    ///     IRC message command.
    /// </summary>
    public enum IrcCommand : ushort
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    ///     Class representing an IRC message.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class IrcMessage
    {
        /// <summary>
        ///     Message tags.
        /// </summary>
        public MessageTags? Tags { get; init; }
        /// <summary>
        ///     Hostmask.
        /// </summary>
        public string? Hostmask { get; init; }
        /// <summary>
        ///     IRC command.
        /// </summary>
        public IrcCommand Command { get; init; }
        /// <summary>
        ///     Message argument.
        /// </summary>
        public string? Arg { get; init; }
        /// <summary>
        ///     Message content.
        /// </summary>
        public MessageContent? Content { get; init; }

        /// <summary>
        ///     Initializes a new <see cref="IrcMessage"/> instance with default values.
        /// </summary>
        public IrcMessage() { }

        /// <summary>
        ///     Parses the <paramref name="input"/> into an instance of <see cref="IrcMessage"/>.
        /// </summary>
        /// <param name="input">Raw IRC message.</param>
        /// <returns><see cref="IrcMessage"/> instance parsed from <paramref name="input"/>.</returns>
        /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
        public static IrcMessage Parse(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
                throw new ArgumentException("Argument cannot be empty", nameof(input));

            MessageTags? tags;
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

                tags = MessageTags.Parse(input[..i]);
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

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = new StringBuilder();

            if (Tags is { Count: > 0 })
                result
                    .Append('@')
                    .Append(Tags)
                    .Append(' ');

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
