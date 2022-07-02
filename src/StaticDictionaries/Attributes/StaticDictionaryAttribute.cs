namespace StaticDictionaries.Attributes;

/// <summary>
/// Attribute creates generated static dictionary from you enum.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class StaticDictionaryAttribute : Attribute
{
    public StaticDictionaryAttribute(params string[] propertyNames)
    {
    }
}