using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Teraa.Irc.Parsing;

namespace Teraa.Irc;

/// <inheritdoc />
[PublicAPI]
public class LazyTags : ITags
{
    private static readonly ITagsParser s_parser = new TagsParser();

    private ITags? _tags;

    /// <summary>
    ///     Initializes a new <see cref="Tags"/> instance with the provided tags <see cref="string"/>.
    /// </summary>
    /// <param name="tags">A <see cref="string"/> containing tag key/value pairs.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tags"/> is null.</exception>
    public LazyTags(string tags)
    {
        RawValue = tags ?? throw new ArgumentNullException(nameof(tags));
    }

    /// <summary>
    ///     Raw <see cref="string"/> representing the tags.
    /// </summary>
    public string RawValue { get; }

    private ITags Initialize()
        => _tags ??= s_parser.Parse(RawValue);

    /// <inheritdoc />
    public string this[string key]
    {
        get
        {
            if (_tags is not null)
                return _tags[key];

            int i = RawValue.LastIndexOf(key, StringComparison.Ordinal);
            if (i == -1 || (i != 0 && RawValue[i - 1] != ';'))
                throw new KeyNotFoundException($"Key '{key}' was not found.");

            int j = RawValue.IndexOf(';', i + key.Length);

            return j == -1
                ? RawValue[i..]
                : RawValue[i..j];
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> Keys
        => Initialize().Keys;

    /// <inheritdoc/>
    public IEnumerable<string> Values
        => Initialize().Values;

    /// <inheritdoc/>
    public int Count
        => Initialize().Count;

    /// <inheritdoc/>
    public bool ContainsKey(string key)
        => Initialize().ContainsKey(key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        => Initialize().GetEnumerator();

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => Initialize().TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => Initialize().GetEnumerator();
}
