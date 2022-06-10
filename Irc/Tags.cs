using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Teraa.Irc;

/// <summary>
///     Type representing a collection of key/value pairs of <see cref="IMessage"/> tags.
/// </summary>
[PublicAPI]
public interface ITags : IReadOnlyDictionary<string, string> { }

/// <inheritdoc />
[PublicAPI]
public class Tags : ITags
{
    private readonly IReadOnlyDictionary<string, string> _tags;

    /// <summary>
    ///     Initializes a new <see cref="Tags"/> instance with the provided <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="tags">A <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing tag key/value pairs.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tags"/> is null.</exception>
    public Tags(IReadOnlyDictionary<string, string> tags)
        => _tags = tags ?? throw new ArgumentNullException(nameof(tags));

    /// <inheritdoc/>
    public string this[string key]
        => _tags[key];

    /// <inheritdoc/>
    public IEnumerable<string> Keys
        => _tags.Keys;

    /// <inheritdoc/>
    public IEnumerable<string> Values
        => _tags.Values;

    /// <inheritdoc/>
    public int Count
        => _tags.Count;

    /// <inheritdoc/>
    public bool ContainsKey(string key)
        => _tags.ContainsKey(key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        => _tags.GetEnumerator();

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => _tags.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => _tags.GetEnumerator();
}
