using System.Collections.Immutable;
using System.Text;
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
    [InlineData("Method Name")]
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

    [Theory]
    [InlineData("string value")]
    [InlineData("4090@#*&^768<>/...,<>./...,<?:;\\\";l;")]
    [InlineData("               ")]
    [InlineData("")]
    [InlineData("True")]
    [InlineData("false")]
    [InlineData("NULL")]
    [InlineData("\n\r\\\"|\"\\   ;")]
    [InlineData(777)]
    [InlineData(123.68)]
    [InlineData(5656.8)]
    [InlineData(-676.111122)]
    [InlineData(0.000)]
    [InlineData(0)]
    [InlineData(-1.0)]
    [InlineData(99999999.8)]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData('.')]
    [InlineData('中')]
    [InlineData('\\')]
    [InlineData('!')]
    [InlineData(' ')]
    [InlineData(';')]
    public void CheckAllSupportedPrimitiveTypes(object typeValue)
    {
        string input = $@"
using StaticDictionaries.Attributes;
using System.Xml.Serialization;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [XmlRoot(""root"")]
        [StaticDictionary(""Property"")]
        public enum Mobile
        {{
            [Value({SymbolDisplay.FormatPrimitive(typeValue, true, false)})]
            Volvo = 1
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("Mobile");
        diagnostics.Should().BeNullOrEmpty();
    }

    [Fact]
    public void PropertyNamesMustNotBeDuplicated()
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Property"", ""Property"")]
        public enum Mobile
        {{
            [Value(true, false)]
            Apple = 1
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.First().GetMessage().Should().Contain("Property names must not be dublicated");
    }

    [Fact]
    public void DifferentParameterTypes_MustThrowException()
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Property"")]
        public enum Mobile
        {{
            [Value(true)]
            Apple = 1,
            [Value('c')]
            Windows = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.First().GetMessage().Should().Contain("has incorrect attribute parameter type. Enum:");
    }

    [Fact]
    public void AllEnumMembersMustHaveValueAttribute()
    {
        const string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Property"")]
        public enum Mobile
        {{
            [Value(true)]
            Apple = 1,
            Windows = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.First().GetMessage().Should().Contain("All `StaticDictionary` enum members must have `Value` attribyte.");
    }

    [Fact]
    public void EnumWithoutStaticDictionariesAttributeButWithValueAttributesMustBeIgnored()
    {
        const string input = @"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        public enum Mobile
        {{
            [Value(true)]
            Apple = 1,
            [Value(false)]
            Windows = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeEmpty();
        diagnostics.Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("internal")]
    public void AllAccessModifierMustWorkCorrect(string accessMidifiedName)
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Real"")]
        {accessMidifiedName} enum MyEnum
        {{
            [Value(true)]
            Apple = 1,
            [Value(false)]
            Windows = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("MyEnum");
        diagnostics.Should().BeNullOrEmpty();
    }

    [Fact]
    public void EnumWithAnotherAttributeMustBeIgnored()
    {
        const string input = @"

using StaticDictionaries.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace StaticDictionaries.Tests.StaticDictionaries
{

    [Serializable]
    public enum EnumWithAnotherAttribute
    {
        [CLSCompliant(true)]
        Member1,
        Member2
    }
}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void BigEnumMustWorkCorrect(int membersQuantity)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < membersQuantity; i++)
        {
            sb.AppendLine($"[Value({(i % 2 == 0).ToString().ToLower()}, \"{Guid.NewGuid()}\")]");
            sb.AppendLine($"Member{i} = {i},");
        }

        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Real"", ""Payload"")]
        public enum Qwerty
        {{
            {sb}
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("Qwerty");
        output.Should().Contain((membersQuantity - 1).ToString());
        diagnostics.Should().BeNullOrEmpty();
    }
}