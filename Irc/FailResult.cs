using System.Runtime.CompilerServices;

namespace Teraa.Irc;

internal enum FailResult
{
    None = 0,

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

internal static class FailResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? FailResultToString(this FailResult value)
    {
        return value switch
        {
            FailResult.CommandEmpty => "Command is empty",
            FailResult.CommandFormat => "Invalid command format",

            FailResult.ContentEmpty => "Content is empty",
            FailResult.ContentMissingCtcpEnding => "Missing content CTCP ending",

            FailResult.PrefixEmpty => "Prefix is empty",
            FailResult.PrefixEmptyHost => "Prefix host is empty",
            FailResult.PrefixEmptyUser => "Prefix user is empty",
            FailResult.PrefixEmptyName => "Prefix name is empty",

            FailResult.TagsEmpty => "Tags are empty",
            FailResult.TagsTrailingSemicolon => "Trailing tags semicolon",
            FailResult.TagsKeyEmpty => "A tag key is empty",

            FailResult.MessageEmpty => "Message is empty",
            FailResult.MessageNoCommandMissingTagsEnding => "Missing command (no tags ending)",
            FailResult.MessageNoCommandAfterTagsEnding => "Missing command (nothing after tags ending)",
            FailResult.MessageNoCommandMissingPrefixEnding => "Missing command (no prefix ending)",
            FailResult.MessageNoCommandAfterPrefixEnding => "Missing command (nothing after prefix ending)",
            FailResult.MessageTrailingSpaceAfterCommand => "Trailing space after command",

            _ => null,
        };
    }
}
