using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Teraa.Irc.Parsing;

/// <inheritdoc />
[PublicAPI]
public class LazyTagsParser : ITagsParser
{
    private static readonly ITagsParser s_parser = new TagsParser();

    /// <inheritdoc />
    public ITags Parse(ReadOnlySpan<char> input)
    {
        return new LazyTags(input.ToString());
    }

    /// <inheritdoc />
    public ITags Parse(string input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return new LazyTags(input);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out ITags? result)
    {
        result = new LazyTags(input.ToString());
        return true;
    }

    /// <inheritdoc />
    public bool TryParse(string? input, [NotNullWhen(true)] out ITags? result)
    {
        if (input is null)
        {
            result = null;
            return false;
        }

        result = new LazyTags(input);
        return true;
    }

    /// <inheritdoc />
    public string ToString(ITags tags)
    {
        return tags is LazyTags lazy
            ? lazy.RawValue
            : s_parser.ToString(tags);
    }
}
