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

        EnumWithFlagsAsStaticDictionaryExtensions.All().Length.Should().Be(7);
        EnumWithFlagsAsStaticDictionaryExtensions.MaxId.Should().Be(7);
        EnumWithFlagsAsStaticDictionaryExtensions.MinId.Should().Be(1);
    }

    [Fact]
    public void EnumInheritedFromByteMustWorkCorrect()
    {
        EnumInheritedFromByte.Brad.Name().Should().Be("Brad");
        EnumInheritedFromByte.Kevin.Id().Should().Be(255);
    }

    [Fact]
    public void EnumInheritedFromShortMustWorkCorrect()
    {
        EnumInheritedFromShort.Brad.Name().Should().Be("Brad");
        EnumInheritedFromShort.Kevin.Id().Should().Be(255);
    }
}