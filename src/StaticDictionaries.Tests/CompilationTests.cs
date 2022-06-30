using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using StaticDictionaries.SourceGeneration;
using StaticDictionaries.Tests.Helpers;
using Xunit;

namespace StaticDictionaries.Tests;

public class CompilationTests
{
    [Theory]
    [InlineData("User", 1, 2, true)]
    [InlineData("Person", 12345, 99, false)]
    [InlineData("employEE", 0, -100, true)]
    [InlineData("ElemeNTs", 9999999, -900, false)]
    [InlineData("Laptops", 000, -900, true)]
    [InlineData("Class_Name", 77, -0, false)]
    [InlineData("cLLass__Name__qwerty", 770, -01, true)]
    [InlineData("className_12_Name____Enum", 770, -01, false)]
    [InlineData("class__Name_99999999999_______N1a2me__21__Enum_", 770, -01, true)]
    public void CanGenerateInStandardCase(string enumName, int age1, int age2, bool active)
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary(""Name"", ""Age"", ""Active"")]
public enum {enumName}
{{
    [Value(""Leonel"", {age1}, {active.ToString().ToLower()})]
    Leo = 1,
    [Value(""Santonel"", {age2}, {active.ToString().ToLower()})]
    Santos = 2
}}";
        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        diagnostics.Should().BeEmpty();
        output.Should().Contain(enumName);
    }

    [Theory]
    [InlineData("User", '_')]
    [InlineData("_    /", '.')]
    [InlineData("\"\"    ''", 't')]
    [InlineData("\"\"   .,..,c.gf '''''' \"\"\"\"   \n \\ <   > ;''  ;", '\\')]
    [InlineData("    <   > ;''  ;", '@')]
    [InlineData("    <   > ;''  ;жжвв  й я^ %)(*&^%$#@!", ' ')]
    [InlineData("    <   > ;\n\n\r\' ////  \\  ;жжвв  й я^ %)(*&^%$#@!", '\n')]
    public void SpecialEscapingSymbols(string password, char symbol)
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary(""Password"", ""Symbol"")]
public enum MyEnum
{{
    [Value({SymbolDisplay.FormatPrimitive(password, true, false)}, {SymbolDisplay.FormatPrimitive(symbol, true, false)})]
    Leo = 1,
    [Value({SymbolDisplay.FormatPrimitive(password, true, false)}, {SymbolDisplay.FormatPrimitive(symbol, true, false)})]
    Santos = 2
}}";
        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().NotBeNull();
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void IgnoreSimpleEnumWithoutAttributes()
    {
        const string input = $@"
namespace StaticDictionaries.Tests.StaticDictionaries;

public enum MyEnum
{{
    Leo = 1,
    Santos = 2
}}";
        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        diagnostics.Should().BeEmpty();
        output.Should().BeEmpty();
    }

    [Fact]
    public void AtttributeParametersQuantityNotEqualsMemberParametersQuantity_MustThrowException()
    {
        const string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary(""Name"", ""Active"")]
public enum IncorrectEnum
{{
        [Value(""Leonel"", ""false"", ""false"")]
        Leo = 1,
        [Value(""Santonel"", ""true"", ""true"")]
        Santos = 2
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.First().GetMessage().Should().Contain("Exception was of type 'ArgumentException' with message '`StaticDictionary`");
    }

    [Theory]
    [InlineData("___")]
    [InlineData("';;'xxs")]
    [InlineData("Метод")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Name ")]
    [InlineData(" Name")]
    [InlineData(" NameName2")]
    [InlineData(" ")]
    public void IncrorrectStaticDictionaryAttributeArgumentsName_MustThrowException(string incorrectArgument)
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary(""Name"", ""{incorrectArgument}"")]
public enum IncorrectEnum
{{
        [Value(""Leonel"", ""false"")]
        Leo = 1,
        [Value(""Santonel"", ""true"")]
        Santos = 2
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.First().GetMessage().Should().Contain("has incorrect name for generation at IncorrectEnum.");
    }

    [Fact]
    public void UniqueNameEnumsInDifferentNameSpaces()
    {
        const string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries1
{{
        [StaticDictionary(""Name"", ""Active"")]
        public enum Enumer
        {{
            [Value(""Leonel"", ""false"")]
            Leo = 1,
            [Value(""Santonel"", ""true"")]
            Santos = 2
        }}
}}

namespace StaticDictionaries.Tests.StaticDictionaries2
{{
        [StaticDictionary(""Name"", ""Active"")]
        public enum Enumer
        {{
            [Value(""Leonel"", ""false"")]
            Leo = 1,
            [Value(""Santonel"", ""true"")]
            Santos = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("Enumer");
        diagnostics.Should().BeNullOrEmpty();
    }
}