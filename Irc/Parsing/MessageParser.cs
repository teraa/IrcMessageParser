using System;
using System.Text;
using JetBrains.Annotations;

namespace Teraa.Irc.Parsing;

/// <summary>
///     <see cref="IMessage"/> parser.
/// </summary>
[PublicAPI]
public interface IMessageParser
{
    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="IMessage"/>.
    /// </summary>
    /// <param name="input">Raw IRC message.</param>
    /// <returns><see cref="IMessage"/> instance parsed from <paramref name="input"/>.</returns>
    IMessage Parse(ReadOnlySpan<char> input);

    /// <inheritdoc cref="Parse(System.ReadOnlySpan{char})"/>
    IMessage Parse(string input);

    /// <summary>
    ///     Parses the <paramref name="input"/> into an instance of <see cref="IMessage"/>.
    /// </summary>
    /// <param name="input">Input to parse.</param>
    /// <param name="result">parsed message if method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was parsed successfully; otherwise, <see langword="false"/>.</returns>
    bool TryParse(ReadOnlySpan<char> input, out IMessage result);

    /// <inheritdoc cref="TryParse(System.ReadOnlySpan{char},out Teraa.Irc.IMessage)"/>
    bool TryParse(string? input, out IMessage result);

    string ToString(IMessage message);
}

/// <inheritdoc />
[PublicAPI]
public class MessageParser : IMessageParser
{
    /// <summary>
    /// <see cref="Command"/> parser.
    /// </summary>
    public ICommandParser CommandParser { get; set; } =  new CommandParser();
    /// <summary>
    /// <see cref="ITags"/> parser.
    /// </summary>
    public ITagsParser TagsParser { get; set; } =  new TagsParser();
    /// <summary>
    /// <see cref="IPrefix"/> parser.
    /// </summary>
    public IPrefixParser PrefixParser { get; set; } =  new PrefixParser();
    /// <summary>
    /// <see cref="IContent"/> parser.
    /// </summary>
    public IContentParser ContentParser { get; set; } =  new ContentParser();

    /// <inheritdoc />
    /// <exception cref="FormatException"><paramref name="input"/> is not in a valid format.</exception>
    public IMessage Parse(ReadOnlySpan<char> input)
    {
        Result parseResult = Parse(input, out IMessage result);
        if (parseResult == Result.Success)
            return result;

        string message = parseResult switch
        {
            Result.Empty => "Input is empty",
            Result.InvalidCommand => "Command is not in a valid format",
            Result.InvalidContent => "Content is not in a valid format",
            Result.InvalidPrefix => "Prefix is not in a valid format",
            Result.InvalidTags => "Tags are not in a valid format",
            Result.NoCommandMissingTagsEnding => "Missing command (no tags ending)",
            Result.NoCommandAfterTagsEnding => "Missing command (nothing after tags ending)",
            Result.NoCommandMissingPrefixEnding => "Missing command (no prefix ending)",
            Result.NoCommandAfterPrefixEnding => "Missing command (nothing after prefix ending)",
            Result.TrailingSpaceAfterCommand => "Trailing space after command",

            _ => parseResult.ToString(),
        };

        throw new FormatException(message);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public IMessage Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input.AsSpan());
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, out IMessage result)
        => Parse(input, out result) == Result.Success;

    /// <inheritdoc />
    public bool TryParse(string? input, out IMessage result)
        => TryParse(input.AsSpan(), out result);

    internal Result Parse(ReadOnlySpan<char> input, out IMessage result)
    {
        result = null!;

        if (input.IsEmpty)
            return Result.Empty;

        ITags? tags;
        IPrefix? prefix;
        Command command;
        string? arg;
        IContent? content;

        int i;

        // Tags
        if (input[0] == '@')
        {
            input = input[1..];
            i = input.IndexOf(' ');

            if (i == -1)
                return Result.NoCommandMissingTagsEnding;

            if (!TagsParser.TryParse(input[..i], out tags))
                return Result.InvalidTags;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return Result.NoCommandAfterTagsEnding;
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
                return Result.NoCommandMissingPrefixEnding;

            if (!PrefixParser.TryParse(input[..i], out prefix))
                return Result.InvalidPrefix;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return Result.NoCommandAfterPrefixEnding;
        }
        else
        {
            prefix = null;
        }

        // Command
        i = input.IndexOf(' ');
        if (i != -1)
        {
            if (!CommandParser.TryParse(input[..i], out command))
                return Result.InvalidCommand;

            input = input[(i + 1)..];

            if (input.IsEmpty)
                return Result.TrailingSpaceAfterCommand;

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
                if (!ContentParser.TryParse(input, out content))
                    return Result.InvalidContent;
            }
        }
        else
        {
            if (!CommandParser.TryParse(input, out command))
                return Result.InvalidCommand;

            arg = null;
            content = null;
        }

        result = new Message(command, tags, prefix, arg, content);

        return Result.Success;
    }

    /// <inheritdoc />
    public string ToString(IMessage message)
    {
        StringBuilder result = new();

        if (message.Tags is { Count: > 0 })
            result
                .Append('@')
                .Append(TagsParser.ToString(message.Tags))
                .Append(' ');

        if (message.Prefix is not null)
            result
                .Append(':')
                .Append(PrefixParser.ToString(message.Prefix))
                .Append(' ');

        result.Append(CommandParser.ToString(message.Command));

        if (message.Arg is not null)
            result
                .Append(' ')
                .Append(message.Arg);

        if (message.Content is not null)
            result
                .Append(" :")
                .Append(ContentParser.ToString(message.Content));


        return result.ToString();
    }

    internal enum Result
    {
        Success = 0,
        Empty,
        InvalidCommand,
        InvalidContent,
        InvalidPrefix,
        InvalidTags,
        NoCommandMissingTagsEnding,
        NoCommandAfterTagsEnding,
        NoCommandMissingPrefixEnding,
        NoCommandAfterPrefixEnding,
        TrailingSpaceAfterCommand,
    }
}
