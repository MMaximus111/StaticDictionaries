using StaticDictionaries.Attributes;

namespace StaticDictionaries.IntegrationTests.StaticDictionaries;

[StaticDictionary("Id", "HistoryName", "Price", "Active", "AverageProductPrice")]
public enum Brands
{
    [Value(4, "Mac", 999_999, true, 900.45)]
    Apple = 1,
    [Value(1, "Samsunger", 777_777, true, 340.45)]
    Samsung = 2
}