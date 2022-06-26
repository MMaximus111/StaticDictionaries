using FluentAssertions;
using StaticDictionaries.Tests.StaticDictionaries;
using Xunit;

namespace StaticDictionaries.Tests;

public class MainTests
{
    [Fact]
    public void InitializeTest()
    {
        Users.Maxim.Id.Should().Be(1);
        Users.John.Id.Should().Be(2);

        Users.Maxim.Name.Should().Be("Maxim");
        Users.John.Name.Should().Be("John");
    }
}