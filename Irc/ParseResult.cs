using System.Runtime.CompilerServices;

namespace Teraa.Irc;

internal enum ParseResult
{
    Success = 0,

    CommandEmpty,
    CommandFormat,

    ContentEmpty,
    ContentMissingCtcpEnding,

    PrefixEmpty,
    PrefixEmptyHost,
    PrefixEmptyUser,
    PrefixEmptyName,

    TagsEmpty,
    TagsTrailingSemicolon,
    TagsKeyEmpty,

    MessageEmpty,
    MessageNoCommandMissingTagsEnding,
    MessageNoCommandAfterTagsEnding,
    MessageNoCommandMissingPrefixEnding,
    MessageNoCommandAfterPrefixEnding,
    MessageTrailingSpaceAfterCommand,
}

internal static class ParseResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ParseResultToString(this ParseResult value)
    {
        return value switch
        {
            ParseResult.CommandEmpty => "Command is empty",
            ParseResult.CommandFormat => "Invalid command format",

            ParseResult.ContentEmpty => "Content is empty",
            ParseResult.ContentMissingCtcpEnding => "Missing content CTCP ending",

            ParseResult.PrefixEmpty => "Prefix is empty",
            ParseResult.PrefixEmptyHost => "Prefix host is empty",
            ParseResult.PrefixEmptyUser => "Prefix user is empty",
            ParseResult.PrefixEmptyName => "Prefix name is empty",

            ParseResult.TagsEmpty => "Tags are empty",
            ParseResult.TagsTrailingSemicolon => "Trailing tags semicolon",
            ParseResult.TagsKeyEmpty => "A tag key is empty",

            ParseResult.MessageEmpty => "Message is empty",
            ParseResult.MessageNoCommandMissingTagsEnding => "Missing command (no tags ending)",
            ParseResult.MessageNoCommandAfterTagsEnding => "Missing command (nothing after tags ending)",
            ParseResult.MessageNoCommandMissingPrefixEnding => "Missing command (no prefix ending)",
            ParseResult.MessageNoCommandAfterPrefixEnding => "Missing command (nothing after prefix ending)",
            ParseResult.MessageTrailingSpaceAfterCommand => "Trailing space after command",

            _ => value.ToString(),
        };
    }
}
