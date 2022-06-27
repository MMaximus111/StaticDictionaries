using FluentAssertions;
using StaticDictionaries.Tests.StaticDictionaries;
using Xunit;

namespace StaticDictionaries.Tests;

public class MainTests
{
    [Fact]
    public void InitializeTest()
    {
        Employee.Maxim.Name();
    }
}