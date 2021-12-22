using System;
using System.Text;

namespace Teraa.IrcMessageParser;

/// <summary>
///     Class representing an IRC message.
///     Message format is defined in <see href="https://datatracker.ietf.org/doc/html/rfc1459#section-2.3.1">RFC 1459 Section 2.3.1</see>.
/// </summary>
public class Message
{
    /// <summary>
    ///     Message tags.
    /// </summary>
    public Tags? Tags { get; init; }
    /// <summary>
    ///     Message prefix.
    /// </summary>
    public Prefix? Prefix { get; init; }
    /// <summary>
    ///     IRC command.
    /// </summary>
    public Command Command { get; init; }
    /// <summary>
    ///     Message argument.
    /// </summary>
    public string? Arg { get; init; }
    /// <summary>
    ///     Message content.
    /// </summary>
    public Content? Content { get; init; }

    /// <summary>
    ///     Initializes a new <see cref="Message"/> instance with default values.
    /// </summary>
    public Message() { }

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Message"/>.
    /// </summary>
    /// <param name="input">Raw IRC message.</param>
    /// <returns><see cref="Message"/> instance parsed from <paramref name="input"/>.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
    public static Message Parse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            throw new ArgumentException("Argument cannot be empty", nameof(input));

        Tags? tags;
        Prefix? prefix;
        Command command;
        string? arg;
        Content? content;

        int i;

        // Tags
        if (input[0] == '@')
        {
            input = input[1..];
            i = input.IndexOf(' ');

            if (i == -1)
                throw new FormatException("Missing tags ending");

            tags = Tags.Parse(input[..i]);
            input = input[(i + 1)..];
        }
        else
        {
            tags = null;
        }

        // Prefix
        if (input[0] == ':')
        {
            input = input[1..];
            i = input.IndexOf(' ');

            if (i == -1)
                throw new FormatException("Missing prefix ending");

            prefix = Prefix.Parse(input[..i]);
            input = input[(i + 1)..];
        }
        else
        {
            prefix = null;
        }

        // Command
        i = input.IndexOf(' ');
        if (i != -1)
        {
            command = CommandParser.Parse(input[..i]);
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
                : Content.Parse(input);
        }
        else
        {
            command = CommandParser.Parse(input);
            arg = null;
            content = null;
        }

        return new Message
        {
            Arg = arg,
            Command = command,
            Content = content,
            Prefix = prefix,
            Tags = tags
        };
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

        if (Prefix is not null)
            result
                .Append(':')
                .Append(Prefix)
                .Append(' ');

        result.Append(CommandParser.ToString(Command));

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
}
