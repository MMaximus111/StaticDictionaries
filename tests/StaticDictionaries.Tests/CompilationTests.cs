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
    [InlineData("q1w2e3r4", 0, -01.0001, true)]
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
    [InlineData("USER", ';')]
    [InlineData("_    /", '.')]
    [InlineData("\"\"    ''", 't')]
    [InlineData("\"\"   .,..,c.gf '''''' \"\"\"\"   \n \\ <   > ;''  ;", '\\')]
    [InlineData("    <   > ;''  ;", '@')]
    [InlineData("    <   > ;''  ;жжвв  й я^ %)(*&^%$#@!", ' ')]
    [InlineData("    <   > ;\n\n\r\' ////  \\  ;жжвв  й я^ %)(*&^%$#@!", '\n')]
    [InlineData(@"    < \\ \;+  > ;\n\n\r\' ////  \\  ;жжвв  й я^ %)(*&^%$#@!", '"')]
    [InlineData($"    < \\ \\+  > ;\n\n\r\' ////  \\  ;жжвв  й я*&[]ХЪ{{@! ; ;;;;", '\r')]
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
    [InlineData("Мethod")]
    [InlineData("Method     ")]
    [InlineData("2qqqqqqqq_qqqqqqqqq")]
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
    [InlineData(" ")]
    [InlineData("True")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("FALSE")]
    [InlineData("NULL")]
    [InlineData("null")]
    [InlineData("\n\r\\\"|\"\\   ;")]
    [InlineData(777)]
    [InlineData(123.68)]
    [InlineData(5656.8)]
    [InlineData(-676.111122)]
    [InlineData(0.000)]
    [InlineData(0)]
    [InlineData(-1.0)]
    [InlineData(99999999.8)]
    [InlineData(99999999999.00)]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData('.')]
    [InlineData('中')]
    [InlineData('\\')]
    [InlineData('!')]
    [InlineData(' ')]
    [InlineData(';')]
    [InlineData('1')]
    [InlineData('\'')]
    [InlineData('\n')]
    [InlineData(@"\ ' "" \\ {{}}")]
    [InlineData(@$"\ '' ; "";"" \\\\ \n \r [ й qwerty {{}}")]
    [InlineData(@$""" }};")]
    [InlineData($"}}")]
    [InlineData("}{")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890")]
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
        const string input = $@"
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

    [Theory]
    [InlineData("'1'", "\"#\"")]
    [InlineData("true", "\"false\"")]
    [InlineData("\"\"", "123")]
    [InlineData("777.1", "'_'")]
    [InlineData("'0'", "0")]
    [InlineData("true", "1")]
    [InlineData("false", "0")]
    [InlineData("\"qwerty\"", "1")]
    [InlineData("0.0", "false")]
    [InlineData("true", "\"true\"")]
    [InlineData("false", "\"false\"")]
    [InlineData("7", "'7'")]
    [InlineData("7", "\"\"")]
    [InlineData("'7'", "1")]
    public void DifferentParameterTypes_MustThrowException(string param1, string param2)
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Property"")]
        public enum Mobile
        {{
            [Value({param1})]
            Apple = 1,
            [Value({param2})]
            Windows = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.First().GetMessage().Should().Contain("has incorrect attribute parameter type. Enum:");
    }

    [Theory]
    [InlineData("17", "1.0")]
    [InlineData("55", "55.55")]
    [InlineData("0", "0.0")]
    [InlineData("77.7", "77")]
    [InlineData("\"12\"", "\"12\"")]
    [InlineData("\"qwerty\"", "\"google\"")]
    [InlineData("0.0", "0")]
    [InlineData("true", "false")]
    [InlineData("false", "true")]
    [InlineData("'1'", "'9'")]
    [InlineData("777777.77", "777777")]
    [InlineData("\"\"", "\"\"")]
    [InlineData("'.'", "'@'")]
    public void SimilarParameterTypes_MustWorkCorrect(string param1, string param2)
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Property"")]
        public enum Tester
        {{
            [Value({param1})]
            Apple = 1,
            [Value({param2})]
            Windows = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("Tester");
        diagnostics.Should().BeNullOrEmpty();
    }

    [Fact]
    public void IntAndDoubleOnOnePositionMustWorkCorrect()
    {
        const string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary(""Name"", ""Price"", ""Price2"")]
    public enum EnumWithIntAndDouble
    {{
        [Value(""Name1"", 67.89, 100)]
        Member1 = 1,
        [Value(""Name2"", 67, 100.123)]
        Member2 = 2
    }}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("EnumWithIntAndDouble");
        diagnostics.Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData("17", "1.0")]
    [InlineData("55", "55.55")]
    [InlineData("0", "0.0")]
    [InlineData("77.7", "77")]
    [InlineData("\"12\"", "\"12\"")]
    [InlineData("0.0", "0")]
    [InlineData("true", "false")]
    [InlineData("false", "true")]
    [InlineData("'1'", "'9'")]
    [InlineData("777777.77", "777777")]
    [InlineData("\"\"", "\"\"")]
    [InlineData("'.'", "'@'")]
    public void SimilarParameterTypes_WithMoreThanOneArgument_MustWorkCorrect(string param1, string param2)
    {
        string input = $@"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [StaticDictionary(""Property"", ""Text"", ""Active"", ""Symbol"")]
        public enum Tester
        {{
            [Value(""qwerty"", {param1}, true, 'q')]
            Apple = 1,
            [Value(""text"", {param2}, false, '9')]
            Windows = 2
        }}
}}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("Tester");
        diagnostics.Should().BeNullOrEmpty();
    }

    [Fact]
    public void NotAllEnumMembersMustHaveValueAttribute()
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

        output.Should().NotBeEmpty();
        diagnostics.Should().BeNullOrEmpty();
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

    [Fact]
    public void EnumWithoutStaticDictionariesAttributeButWithXmlSupportAttributesMustBeIgnored()
    {
        const string input = @"
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{{
        [XmlSupport]   
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
    [InlineData(20000)]
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

    [Fact]
    public void EnumWithoutValuesAttributesMustBeIgnored()
    {
        const string input = @"

using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{
   [StaticDictionary]
    public enum EnumWithoutSettedIdAndNameAndWithoutValues
    {
        Value1 = 1,
        Value2 = 2
    }
}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().BeNullOrEmpty();
        diagnostics.Should().BeNullOrEmpty();
    }

    [Fact]
    public void EnumWithJsonAndXmlAttributesMustGenerateCorrect()
    {
        const string input = @"

using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{
    [XmlSupport]
    [JsonSupport]
    [StaticDictionary(""Logo"")]
    public enum EnumWithXmlAndJsonSupport
    {
        [Value(""Logo1"")]
        Value1 = 1,
        [Value(""Logo2"")]
        Value2 = 2
    }
}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("Json()");
        output.Should().Contain("Xml()");

        diagnostics.Should().BeNullOrEmpty();
    }

    [Fact]
    public void EnumWithJsonAndWithoutXmlAttributesMustGenerateOnlyJsonMethod()
    {
        const string input = @"

using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries
{
    [JsonSupport]
    [StaticDictionary(""Logo"")]
    public enum EnumWithXmlAndJsonSupport
    {
        [Value(""Logo1"")]
        Value1 = 1,
        [Value(""Logo2"")]
        Value2 = 2
    }
}";

        (ImmutableArray<Diagnostic> diagnostics, string output) = CompilationTestHelper.GetGeneratedOutput<StaticDictionaryGenerator>(input);

        output.Should().Contain("Json()");
        output.Should().NotContain("Xml()");

        diagnostics.Should().BeNullOrEmpty();
    }
}