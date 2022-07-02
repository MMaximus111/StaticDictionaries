using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary]
public enum StaticDictionaryWithoutArguments
{
    [Value]
    Member1,
    [Value]
    Member2
}