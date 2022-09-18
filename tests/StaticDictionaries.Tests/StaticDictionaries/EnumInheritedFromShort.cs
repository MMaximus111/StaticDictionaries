using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary]
public enum EnumInheritedFromShort : short
{
    [Value]
    Brad = 1,
    [Value]
    Kevin = 255
}