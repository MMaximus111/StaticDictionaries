using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary]
public enum EnumInheritedFromByte : byte
{
    [Value]
    Brad = 1,
    [Value]
    Kevin = 255
}