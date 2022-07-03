using System.Diagnostics.CodeAnalysis;

namespace StaticDictionaries.Attributes;

/// <summary>
/// Attribute creates generated static dictionary from you enum.
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Enum)]
public class StaticDictionaryAttribute : Attribute
{
    public StaticDictionaryAttribute(params string[] propertyNames)
    {
    }
}