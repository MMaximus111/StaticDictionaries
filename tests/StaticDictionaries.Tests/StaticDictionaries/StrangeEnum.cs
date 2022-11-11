using System.ComponentModel;
using Argon;
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Uuid", "Sumbol", "Active", "tExt", "Votes", "priCE")]
[Category]
[ToolboxItem(true)]
public enum StrangeEnum
{
    [Value("343-3434223fdfdf-efefe", '"', true, ",,.;;:*6-+//|\\", 23, 506.7)]
    duck = 1,

    [JsonRequired]
    [Value("fkldfkld4o44904-3", ';', false, "#@#.<>./][[ {   } \\;", 9999999, 111.11)]
    Lion = 24,

    [Browsable(true)]
    [Value("fkldfkld4o44904-3", 'х', false, "7?&88^%$#@!':\\ -()990[];:  .", 2, 10.00)]
    Tiger = 9999,

    [Browsable(true)]
    [Value("fkldfkld4o44904-3", 'х', true, "7?&88^%$#@!':\\ -()990[];:  .", 1, 02.00)]
    Elephant = 1000000
}