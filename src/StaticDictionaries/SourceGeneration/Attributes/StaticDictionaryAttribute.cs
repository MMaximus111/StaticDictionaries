namespace StaticDictionaries.SourceGeneration.Attributes;

[AttributeUsage(AttributeTargets.Enum)]
public class StaticDictionaryAttribute : Attribute
{
    public StaticDictionaryAttribute(string firstPropertyName, params string[] propertyNames)
    {
        PropertyNames = propertyNames.Union(new[] { firstPropertyName }).ToArray();
    }

    public string[] PropertyNames { get; }
}