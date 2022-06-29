using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Name", "Age", "Active")]
public enum Employee
{
    [Value("Максим", 18, true)]
    Maxim = 1,
    [Value("Джон", 23, false)]
    John = 2
}