using StaticDictionaries.Attributes;

namespace StaticDictionaries.SourceGeneration;

public static class AttributeConstants
{
    public const string StaticDictionaryAttributeFullName = $"StaticDictionaries.Attributes.{StaticDictionaryAttributeName}";
    public const string StaticDictionaryAttributeName = nameof(StaticDictionaryAttribute);

    public const string ValueAttributeFullName = $"StaticDictionaries.Attributes.{ValueAttributeName}";
    public const string ValueAttributeName = nameof(ValueAttribute);
}