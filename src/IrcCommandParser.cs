using System;

namespace IrcMessageParser
{
    /// <summary>
    ///     Class for parsing <see cref="IrcCommand"/> values.
    /// </summary>
    public static class IrcCommandParser
    {
        /// <summary>
        ///     Parses the <see cref="IrcCommand"/> from <paramref name="input"/>.
        ///     See <see href="https://tools.ietf.org/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see> for details.
        /// </summary>
        /// <param name="input">Input to parse.</param>
        /// <returns><see cref="IrcCommand"/>.</returns>
        /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
        public static IrcCommand Parse(ReadOnlySpan<char> input)
        {
            if (input.Length == 3 && ushort.TryParse(input, out var numeric))
                return (IrcCommand)numeric;

            const IrcCommand maxNumeric = (IrcCommand)999;

#if NET6_0_OR_GREATER
            if (Enum.TryParse<IrcCommand>(input, true, out var command))
#else
            if (Enum.TryParse<IrcCommand>(input.ToString(), true, out var command))
#endif
                if (command is > maxNumeric && (input[0] is (< '0' or > '9')))
                    return command;

            throw new FormatException($"Invalid command format: {input.ToString()}");
        }

        /// <summary>
        ///     Returns the <see cref="string"/> representation of the <see cref="IrcCommand"/>
        /// </summary>
        /// <param name="command">Input command.</param>
        /// <returns><see cref="string"/> representing the command.</returns>
        public static string ToString(IrcCommand command)
        {
            if ((ushort)command is <= 999)
                return ((ushort)command).ToString("d3");

            return command.ToString();
        }
    }
}
