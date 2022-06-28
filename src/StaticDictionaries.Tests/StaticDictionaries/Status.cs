using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using StaticDictionaries.SourceGeneration.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[AmbientValue("qwerty")]
[DataContract]
[StaticDictionary("Name", "IsLast", "FullName", "Comment", "Symbol")]
public enum Status
{
    [Column]
    [NonSerialized]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Value("New state", false, "Full name state", "some data", 'q')]
    New = 1,

    [Value("progress state", true, "#ddd", "qwerty", '_')]
    InProgress = 2,

    [NonSerialized]
    [Value("State text", false, "Full name 12345", "", '\'')]
    Packing = 3,

    [Column]
    [Value("Ready1", true, "34-4jm0posfdllmfdjs", "\'\\'fdlsfdcflkfdcm_-=()", '.')]
    Ready = 4,

    [Column]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [NonSerialized]
    [Value("=-    ds/..\\\\/,.,=ж;;шшвьч[]", true, "92-1-48934243.,.,БЮ<>..//#$-)", "1!$#44z,mz     ,. /\\ \\x", '"')]
    Completed = 5,

    [Column]
    [Value("", true, "3337?;№2!@         %^.;;'''", "1!$#44z,mz  俗字 \"\"\"\"  ,. /\\////][] ХЪ {} \\x\"\"", '\\')]
    Finished = 6,

    [NonSerialized]
    [Value("null", false, "92-1-48934243.,.,БЮ<>..//#$-)", "1!$#44z,mz     ,. /\\ \\x", '1')]
    FuckStatus = 7,

    [NotMapped]
    [Value("Last 异体", true, "92-1-4893$$$$*..4243.,.,БЮ<>..//#$-)", "     異體字    {   .   }}  ; льыж", ']')]
    LastStatus = 8
}