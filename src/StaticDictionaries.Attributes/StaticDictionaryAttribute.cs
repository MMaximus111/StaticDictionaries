using System.Diagnostics.CodeAnalysis;

namespace StaticDictionaries.Attributes;

/// <summary>
/// Attribute creates generated static dictionary from you enum.
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Enum)]
public class StaticDictionaryAttribute : Attribute
{
    /// <summary>
    /// Define names for generated properties.
    /// </summary>
    /// <param name="propertyNames">Names for generated properties.</param>
    public StaticDictionaryAttribute(params string[] propertyNames)
    {
    }
}