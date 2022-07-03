using System.Diagnostics.CodeAnalysis;

namespace StaticDictionaries.Attributes;

/// <summary>
/// Attribute for assigning values on enum member.
/// </summary>

[ExcludeFromCodeCoverage]
public class ValueAttribute : Attribute
{
    public ValueAttribute(params object[] values)
    {
    }
}