using StaticDictionaries.Attributes;

namespace StaticDictionaries.IntegrationTests.StaticDictionaries;

[JsonSupport]
[StaticDictionary]
public enum EnumWithJsonSupport
{
    [Value]
    Member1 = 1,
    [Value]
    Member2 = 2
}