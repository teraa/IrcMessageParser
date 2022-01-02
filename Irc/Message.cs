using System;
using System.Text;

namespace Teraa.Irc;

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

    /// <inheritdoc cref="Parse(ReadOnlySpan{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public static Message Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Message"/>.
    /// </summary>
    /// <param name="input">Raw IRC message.</param>
    /// <returns><see cref="Message"/> instance parsed from <paramref name="input"/>.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public static Message Parse(ReadOnlySpan<char> input)
    {
        ParseStatus status = Parse(input, out Message result);
        if (status is ParseStatus.Success)
            return result;

        string? message = status switch
        {
            ParseStatus.FailEmpty => "Input is empty",
            ParseStatus.FailTags => "Message tags invalid format",
            ParseStatus.FailPrefix => "Message prefix invalid format",
            ParseStatus.FailCommand => "Message command invalid format",
            ParseStatus.FailContent => "Message content invalid format",
            ParseStatus.FailNoCommandMissingTagsEnding => "Missing command (no tags ending)",
            ParseStatus.FailNoCommandAfterTagsEnding => "Missing command (nothing after tags ending)",
            ParseStatus.FailNoCommandMissingPrefixEnding => "Missing command (no prefix ending)",
            ParseStatus.FailNoCommandAfterPrefixEnding => "Missing command (nothing after prefix ending)",
            ParseStatus.FailTrailingSpaceAfterCommand => "Trailing space after command",
            _ => null,
        };

        throw new FormatException(message);
    }

    internal static ParseStatus Parse(ReadOnlySpan<char> input, out Message result)
    {
        result = null!;

        if (input.IsEmpty)
            return ParseStatus.FailEmpty;

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
                return ParseStatus.FailNoCommandMissingTagsEnding;

            if (Tags.Parse(input[..i], out tags) != Tags.ParseStatus.Success)
                return ParseStatus.FailTags;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return ParseStatus.FailNoCommandAfterTagsEnding;
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
                return ParseStatus.FailNoCommandMissingPrefixEnding;

            if (Prefix.Parse(input[..i], out prefix) != Prefix.ParseStatus.Success)
                return ParseStatus.FailPrefix;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return ParseStatus.FailNoCommandAfterPrefixEnding;
        }
        else
        {
            prefix = null;
        }

        // Command
        i = input.IndexOf(' ');
        if (i != -1)
        {
            if (CommandParser.Parse(input[..i], out command) != CommandParser.ParseStatus.Success)
                return ParseStatus.FailCommand;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return ParseStatus.FailTrailingSpaceAfterCommand;

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

            if (input.IsEmpty)
            {
                content = null;
            }
            else
            {
                if (Content.Parse(input, out content) != Content.ParseStatus.Success)
                    return ParseStatus.FailContent;
            }
        }
        else
        {
            if (CommandParser.Parse(input, out command) != CommandParser.ParseStatus.Success)
                return ParseStatus.FailCommand;

            arg = null;
            content = null;
        }

        result = new Message
        {
            Arg = arg,
            Command = command,
            Content = content,
            Prefix = prefix,
            Tags = tags
        };

        return ParseStatus.Success;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder result = new();

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

    internal enum ParseStatus
    {
        Success,
        FailEmpty,
        FailTags,
        FailPrefix,
        FailCommand,
        FailContent,
        FailNoCommandMissingTagsEnding,
        FailNoCommandAfterTagsEnding,
        FailNoCommandMissingPrefixEnding,
        FailNoCommandAfterPrefixEnding,
        FailTrailingSpaceAfterCommand,
    }
}
