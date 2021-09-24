using System;

namespace IrcMessageParser
{
    /// <summary>
    ///     Record representing message prefix of a <see cref="IrcMessage"/>.
    ///     Prefix contains information about the server or user sending the message.
    ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
    /// </summary>
    public record MessagePrefix
    {
        /// <summary>
        ///     Server name or user nick.
        /// </summary>
        public string Name { get; }
        /// <summary>
        ///     User.
        /// </summary>
        public string? User { get; }
        /// <summary>
        ///     Host.
        /// </summary>
        public string? Host { get; }

        /// <summary>
        ///     Initializes a new <see cref="MessagePrefix"/> instance with provided values.
        /// </summary>
        /// <param name="name">.</param>
        /// <param name="user">.</param>
        /// <param name="host">.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        public MessagePrefix(string name, string? user, string? host)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            User = user;
            Host = host;
        }

        /// <summary>
        ///     Parses the <paramref name="input"/> into an instance of <see cref="MessagePrefix"/>.
        ///     See <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
        /// </summary>
        /// <param name="input">Content.</param>
        /// <returns><see cref="MessagePrefix"/> instance parsed from <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
        public static MessagePrefix Parse(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
                throw new ArgumentException("Argument cannot be empty", nameof(input));

            string name;
            string? user, host;

            int i = input.IndexOf('@');

            if (i != -1)
            {
                host = input[(i + 1)..].ToString();
                input = input[..i];
            }
            else
            {
                host = null;
            }

            i = input.IndexOf('!');

            if (i != -1)
            {
                user = input[(i + 1)..].ToString();
                input = input[..i];
            }
            else
            {
                user = null;
            }

            name = input.ToString();

            return new MessagePrefix(name, user, host);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return (User, Host) switch
            {
                (not null, not null) => $"{Name}!{User}@{Host}",
                (not null, null) => $"{Name}!{User}",
                (null, not null) => $"{Name}@{Host}",
                (null, null) => Name,
            };
        }
    }
}
