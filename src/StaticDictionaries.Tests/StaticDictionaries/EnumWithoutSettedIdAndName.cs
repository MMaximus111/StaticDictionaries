using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Uuid")]
public enum EnumWithoutSettedIdAndName
{
    [Value("9458495893")]
    Value1 = 1,
    [Value("9458495893")]
    Value2 = 2
}