using BenchmarkDotNet.Attributes;
using StaticDictionaries.Benchmarks.StaticDictionaries;

namespace StaticDictionaries.Benchmarks;

public class Benchmarks
{
    [Benchmark]
    public void ClassicAccessToMemberName()
    {
        string name = nameof(Cars.Bmw);
    }

    [Benchmark]
    public void GeneratedAccessToMemberName()
    {
        string porscheName = Cars.Bmw.Name();
    }

    [Benchmark]
    public void ReflectionGetAllMembers()
    {
        Array cars = Enum.GetValues(typeof(Cars));
    }

    [Benchmark]
    public void GeneratedGetAllMembers()
    {
        Cars[] cars = CarsExtensions.All();
    }

    [Benchmark]
    public void GetEnumMemberById()
    {
        Cars car = (Cars)3;
    }

    [Benchmark]
    public void GeneratedGetEnumMemberById()
    {
        Cars car = CarsExtensions.GetById(3);
    }
}