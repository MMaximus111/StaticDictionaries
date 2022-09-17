using System.Xml.Serialization;
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[XmlSupport]
[JsonSupport]
[Serializable]
[XmlRoot]
[StaticDictionary("Nick", "Age", "Active", "Cost")]
public enum EnumWithSerializationSupport
{
    [XmlElement]
    [Value("CaesarTurbo", 19, true, 12.23)]
    Caesar = 1,
    [Value("MisterMuscle", 22, false, 77.8)]
    Mister = 2,
    [Value("LobsterNick", 17, true, 1.0)]
    Lobster = 3
}