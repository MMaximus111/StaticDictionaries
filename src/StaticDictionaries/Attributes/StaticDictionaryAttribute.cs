namespace StaticDictionaries.Attributes;

[AttributeUsage(AttributeTargets.Enum)]
public class StaticDictionaryAttribute : Attribute
{
    public StaticDictionaryAttribute(params string[] propertyNames)
    {
        PropertyNames = propertyNames;
    }

    public string[] PropertyNames { get; }
}