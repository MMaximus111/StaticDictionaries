using StaticDictionaries.SourceGeneration;

namespace StaticDictionaries.Tests.StaticDictionaries;

[SourceGeneration]
public class Users : StaticDictionary
{
    private Users(int id, string name)
        : base(id, name)
    {
    }

    public static readonly Users Maxim = new Users(1, "Maxim");

    public static readonly Users John = new Users(2, "John");
}