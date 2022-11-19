using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("ChildCompany", "TotalCost")]
public enum Company123
{
    [Value(GitHub, 1_034_000_250)]
    Microsoft = 1,
    [Value(GitHub, 80_000_000)]
    GitHub = 2,
    [Value(Beats, 2_000_200_000)]
    Apple = 3,
    [Value(GitHub, 70_900_500)]
    Beats = 4
}