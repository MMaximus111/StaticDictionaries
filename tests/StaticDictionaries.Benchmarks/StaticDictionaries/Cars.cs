using StaticDictionaries.Attributes;

namespace StaticDictionaries.Benchmarks.StaticDictionaries;

[StaticDictionary("Price", "Name", "MaxSpeed")]
public enum Cars
{
    [Value(70_000, "Lexus inc.", 255)]
    Lexus = 1,
    [Value(85_000, "Porsche manufactor.", 277)]
    Porsche = 2
}