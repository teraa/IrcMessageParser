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
        FailResult result = Parse(input, out Message message);
        if (result == FailResult.None)
            return message;

        throw new FormatException(result.FailResultToString());
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, out Message)"/>
    public static bool TryParse(string? input, out Message result)
        => TryParse(input.AsSpan(), out result);

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="Message"/>.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed message if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out Message result)
        => Parse(input, out result) == FailResult.None;

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

    internal static FailResult Parse(ReadOnlySpan<char> input, out Message result)
    {
        result = null!;

        if (input.IsEmpty)
            return FailResult.MessageEmpty;

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
                return FailResult.MessageNoCommandMissingTagsEnding;

            var tagsResult = Tags.Parse(input[..i], out tags);
            if (tagsResult != FailResult.None)
                return tagsResult;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return FailResult.MessageNoCommandAfterTagsEnding;
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
                return FailResult.MessageNoCommandMissingPrefixEnding;

            var prefixResult = Prefix.Parse(input[..i], out prefix);
            if (prefixResult != FailResult.None)
                return prefixResult;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return FailResult.MessageNoCommandAfterPrefixEnding;
        }
        else
        {
            prefix = null;
        }

        // Command
        i = input.IndexOf(' ');
        if (i != -1)
        {
            var commandResult = CommandParser.Parse(input[..i], out command);
            if (commandResult != FailResult.None)
                return commandResult;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return FailResult.MessageTrailingSpaceAfterCommand;

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
                var contentResult = Content.Parse(input, out content);
                if (contentResult != FailResult.None)
                    return contentResult;
            }
        }
        else
        {
            var commandResult = CommandParser.Parse(input, out command);
            if (commandResult != FailResult.None)
                return commandResult;

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

        return FailResult.None;
    }
}
