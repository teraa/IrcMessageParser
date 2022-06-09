using BenchmarkDotNet.Attributes;
using Teraa.Irc;
using Teraa.Irc.Parsing;

namespace Irc.Benchmarks;

[MemoryDiagnoser(false)]
public class CommandBenchs
{
    private ICommandParser _defaultParser = null!;
    private ICommandParser _fastParser = null!;

    [ParamsSource(nameof(Data))] public string Input { get; set; } = null!;


    [GlobalSetup]
    public void Setup()
    {
        _defaultParser = new CommandParser();
        _fastParser = new FastCommandParser();
    }

    public IEnumerable<string> Data()
    {
        return new[]
        {
            "CLEARCHAT",
            "200"
        };
    }

    [Benchmark] public Command Enum_Parse() => Enum.Parse<Command>(Input);
    [Benchmark] public Command Default_Parse() => _defaultParser.Parse(Input);
    [Benchmark] public Command Fast_Parse() => _fastParser.Parse(Input);
}
