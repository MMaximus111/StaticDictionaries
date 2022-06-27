namespace StaticDictionaries.SourceGeneration;

public sealed class EnumMemberDefinition
{
    public EnumMemberDefinition(int id, string memberName, object?[] values)
    {
        Id = id;
        MemberName = memberName;
        Values = values;
    }

    public int Id { get; }

    public string MemberName { get; }

    public object?[] Values { get; }
}