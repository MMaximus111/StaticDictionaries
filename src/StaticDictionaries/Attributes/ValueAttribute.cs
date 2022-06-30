namespace StaticDictionaries.Attributes;

/// <summary>
/// Attribute for assigning values on enum member.
/// </summary>
public class ValueAttribute : Attribute
{
    public ValueAttribute(params object[] values)
    {
    }
}