using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Id")]
public enum OverriddenIdProperty
{
    [Value(666)]
    Member1 = 1,
    [Value(555)]
    Member2 = 3,
    [Value(111)]
    Member3 = 777,
    [Value(9999999)]
    Member4 = 4
}