using BenchmarkDotNet.Attributes;
using Teraa.Irc;
using Teraa.Irc.Parsing;

namespace Irc.Benchmarks;

[MemoryDiagnoser(false)]
public class TagsBenchs
{
    private ITagsParser _defaultParser = null!;
    private ITagsParser _lazyParser = null!;

    [ParamsSource(nameof(Data))] public string Input { get; set; } = null!;


    [GlobalSetup]
    public void Setup()
    {
        _defaultParser = new TagsParser();
        _lazyParser = new LazyTagsParser();
    }

    public IEnumerable<string> Data()
    {
        return new[]
        {
            "one=[one];two;three=[three];four",
            "key=value"
        };
    }

    [Benchmark] public ITags Default_Parse() => _defaultParser.Parse(Input);
    [Benchmark] public ITags Lazy_Parse() => _lazyParser.Parse(Input);
}
