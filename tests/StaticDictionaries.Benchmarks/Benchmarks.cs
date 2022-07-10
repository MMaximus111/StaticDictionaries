using System.Reflection;
using BenchmarkDotNet.Attributes;
using StaticDictionaries.Benchmarks.StaticDictionaries;

namespace StaticDictionaries.Benchmarks;

public class Benchmarks
{
    // [Benchmark]
    // public void AccessIntPropertyViaMethod()
    // {
    //     int speed = Cars.Porsche.MaxSpeed();
    // }
    //
    // [Benchmark]
    // public void AccessNamePropertyViaMethod()
    // {
    //     string porscheName = Cars.Porsche.Name();
    // }
    //
    // [Benchmark]
    // public void ClassicAccessOfMemberId()
    // {
    //     int porscheId = (int)Cars.Porsche;
    // }
    //
    // [Benchmark]
    // public void ClassicAccessToMemberName()
    // {
    //     string name = nameof(Cars.Porsche);
    // }
    //
    // [Benchmark]
    // public void GetAllMemberViaGeneratedExtensionClass()
    // {
    //     Cars[] cars = CarsExtensions.All();
    // }
    //
    // [Benchmark]
    // public void GetAllMemberViaReflection()
    // {
    //     PropertyInfo[] carsProperties = typeof(Cars).GetProperties();
    // }

    [Benchmark]
    public void Switch()
    {
        int id = Cars.Porsche.Id();
    }

    [Benchmark]
    public void If()
    {
        int id = Cars.Porsche.IdPoweredByIf();
    }
}