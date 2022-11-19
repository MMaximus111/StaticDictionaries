using StaticDictionaries.Attributes;

namespace StaticDictionaries.IntegrationTests.StaticDictionaries;

[XmlSupport]
[StaticDictionary]
public enum EnumWithXmlSupport
{
    [Value]
    Member1 = 1,
    [Value]
    Member2 = 2
}