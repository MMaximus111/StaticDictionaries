using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Name", "Price", "Price2")]
public enum EnumWithIntAndDouble
{
    [Value("Name1", 67.89, 100)]
    Member1 = 1,
    [Value("Name2", 67, 100.123)]
    Member2 = 2
}