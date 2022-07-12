using System.Diagnostics.CodeAnalysis;

namespace StaticDictionaries.Attributes;

/// <summary>
/// Attribute for assigning values on enum member.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValueAttribute : Attribute
{
    /// <summary>
    /// Defines enum member property values.
    /// </summary>
    /// <param name="values">Enum member property values.</param>
    public ValueAttribute(params object[] values)
    {
    }
}