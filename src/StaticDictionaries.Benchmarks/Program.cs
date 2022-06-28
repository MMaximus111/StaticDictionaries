using BenchmarkDotNet.Running;

namespace StaticDictionaries.Benchmarks;

public class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}