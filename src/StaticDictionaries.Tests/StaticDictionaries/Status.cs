using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using StaticDictionaries.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[XmlRoot("sdsd")]
[AmbientValue("qwerty")]
[DataContract]
[StaticDictionary("Name", "IsLast", "FullName", "Comment", "Symbol")]
public enum Status
{
    [ToolboxItem(true)]
    [NonSerialized]
    [Value("New state", false, "Full name state", "some data", 'q')]
    New = 1,

    [Value("progress state", true, "#ddd", "qwerty", '_')]
    InProgress = 2,

    [NonSerialized]
    [Value("State text", false, "Full name 12345", "", '\'')]
    Packing = 3,

    [Value("Ready1", true, "    <   > ;\n\n\r\' ////  \\  ;жжвв  й я^ %)(*&^%$#@!", "\'\\'fdlsfdcflkfdcm_-=()", '.')]
    Ready = 4,

    [NonSerialized]
    [Value("=-    ds/..\\\\/,.,=ж;;шшвьч[]", true, "92-1-48934243.,.,БЮ<>..//#$-)", "1!$#44z,mz     ,. /\\ \\x", '"')]
    Completed = 5,

    [Value("", true, "3337?;№2!@         %^.;;'''", "1!$#44z,mz  俗字 \"\"\"\"  ,. /\\////][] ХЪ {} \\x\"\"", '\\')]
    Finished = 6,

    [NonSerialized]
    [Value("null", false, "92-1-48934243.,.,БЮ<>..//#$-)", "1!$#44z,mz     ,. /\\ \\x", '1')]
    FuckStatus = 7,

    [XmlElement]
    [DesignOnly(false)]
    [Description]
    [ToolboxItem(true)]
    [Value("Last 异体", true, "92-1-4893$$$$*..4243.,.,БЮ<>..//#$-)", "     異體字    {   .   }}  ; льыж", ']')]
    LastStatus = 8
}