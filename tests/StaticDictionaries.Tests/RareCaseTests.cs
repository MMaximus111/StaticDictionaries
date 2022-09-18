using FluentAssertions;
using StaticDictionaries.Tests.StaticDictionaries;
using Xunit;

namespace StaticDictionaries.Tests;

public class RareCaseTests
{
    [Fact]
    public void EnumWithFlagsMustWorkCorrectWithoutErrors()
    {
        EnumWithFlagsAsStaticDictionary.Friday.Name().Should().Be("Friday");
        EnumWithFlagsAsStaticDictionary.Saturnday.Name().Should().Be("Saturnday");

        EnumWithFlagsAsStaticDictionary.Weekend.Name().Should().Be(default);
        EnumWithFlagsAsStaticDictionary.Weekend.Id().Should().Be(default);

        EnumWithFlagsAsStaticDictionaryExtensions.All().Length.Should().Be(7);
        EnumWithFlagsAsStaticDictionaryExtensions.MaxId.Should().Be(7);
        EnumWithFlagsAsStaticDictionaryExtensions.MinId.Should().Be(1);
    }

    [Fact]
    public void EnumInheritedFromByteMustWorkCorrect()
    {
        
    }
}