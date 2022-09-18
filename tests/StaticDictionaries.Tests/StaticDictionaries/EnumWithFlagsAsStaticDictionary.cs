using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[JsonSupport]
[StaticDictionary]
public enum EnumWithFlagsAsStaticDictionary
{
    [Value]
    Monday = 1,
    [Value]
    Tuesday = 2,
    [Value]
    Wednesday = 3,
    [Value]
    Thursday = 4,
    [Value]
    Friday = 5,
    [Value]
    Saturnday = 6,
    [Value]
    Sunday = Monday | Tuesday,
    [Value]
    Weekend = Wednesday | Thursday
}