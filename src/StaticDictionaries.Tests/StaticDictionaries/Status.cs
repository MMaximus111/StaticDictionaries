using StaticDictionaries.SourceGeneration.Attributes;

namespace StaticDictionaries.Tests.StaticDictionaries;

[StaticDictionary("Name", "IsLast", "FullName", "Comment")]
public enum Status
{
    [Value("New state", false, "Full name state", "some data")]
    New = 1,

    [Value("progress state", true, "Full name in progress state", "qwerty")]
    InProgress = 2,

    [Value("State text", false, "Full name 12345", "")]
    Packing = 3,

    [Value("Ready1", true, "34-4jm0posfdllmfdjs", "\'\\'fdlsfdcflkfdcm_-=()")]
    Ready = 4,

    [Value("=-    ds/..\\\\/,.,=ж;;шшвьч[]", true, "92-1-48934243.,.,БЮ<>..//#$-)", "1!$#44z,mz     ,. /\\ \\x")]
    Completed = 5,

    [Value("", true, "3337?;№2!@         %^.;;'''", "1!$#44z,mz  俗字 \"\"\"\"  ,. /\\////][] ХЪ {} \\x\"\"")]
    Finished = 6,

    [Value("null", false, "92-1-48934243.,.,БЮ<>..//#$-)", "1!$#44z,mz     ,. /\\ \\x")]
    FuckStatus = 7,

    [Value("Last 异体", true, "92-1-4893$$$$*..4243.,.,БЮ<>..//#$-)", "     異體字    {   .   }}  ; льыж")]
    LastStatus = 8
}