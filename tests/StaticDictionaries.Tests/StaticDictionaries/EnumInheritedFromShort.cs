using System.ComponentModel;
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary]
[AmbientValue("qwerty")]
public enum EnumInheritedFromShort : short
{
    [Value]
    Brad = 1,
    [Value]
    Kevin = 255
}