namespace StaticDictionaries.Tests.StaticDictionaries;

[Serializable]
public enum EnumWithAnotherAttribute
{
    [CLSCompliant(true)]
    Member1,
    Member2
}