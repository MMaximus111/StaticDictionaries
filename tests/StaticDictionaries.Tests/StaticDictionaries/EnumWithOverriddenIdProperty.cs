using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Id", "Material")]
public enum EnumWithOverriddenIdProperty
{
    [Value(7, "Gold")]
    Table = 1,
    [Value(19, "Aluminium")]
    Chair = 15
}