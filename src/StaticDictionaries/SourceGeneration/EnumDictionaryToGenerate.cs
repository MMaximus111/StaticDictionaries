namespace StaticDictionaries.SourceGeneration;

public readonly struct EnumDictionaryToGenerate
{
    public EnumDictionaryToGenerate(
        string name,
        string nameSpace,
        IReadOnlyCollection<string> propertyNames,
        List<Type> propertyTypes,
        IReadOnlyCollection<EnumMemberDefinition> members,
        bool isPublic)
    {
        Name = name;
        Namespace = nameSpace;
        Members = members;
        IsPublic = isPublic;
        PropertyNames = propertyNames;
        PropertyTypes = propertyTypes;
    }

    public bool IsPublic { get; }

    public string Name { get; }

    public string Namespace { get; }

    public IReadOnlyCollection<EnumMemberDefinition> Members { get; }

    public IReadOnlyCollection<string> PropertyNames { get; }

    public List<Type> PropertyTypes { get; }
}