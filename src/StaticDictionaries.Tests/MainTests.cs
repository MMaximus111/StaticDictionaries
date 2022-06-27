using FluentAssertions;
using StaticDictionaries.Tests.StaticDictionaries;
using Xunit;

namespace StaticDictionaries.Tests;

public class MainTests
{
    [Fact]
    public void InitializeTest()
    {
        int age = Employee.John.Age();
        bool active = Employee.Maxim.Active();
        string name = Employee.Maxim.Name();

        age.Should().Be(23);
        active.Should().Be(true);
        name.Should().Be("Максим");
    }
}