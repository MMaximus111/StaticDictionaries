using BenchmarkDotNet.Attributes;
using StaticDictionaries.Benchmarks.StaticDictionaries;

namespace StaticDictionaries.Benchmarks;

public class Benchmarks
{
    [Benchmark]
    public void AccessPropertyViaMethod()
    {
        int porscheName = Cars.Porsche.MaxSpeed();
    }

    [Benchmark]
    public void ClassicAccessOfMemberId()
    {
        int porscheId = (int)Cars.Porsche;
    }
}