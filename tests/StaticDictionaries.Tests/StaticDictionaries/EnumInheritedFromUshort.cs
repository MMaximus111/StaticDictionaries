using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Age", "Speed")]
public enum EnumInheritedFromUshort : ushort
{
    [Value(1, 23)]
    Brad = 1,
    [Value(13, 77)]
    Kevin = 255
}