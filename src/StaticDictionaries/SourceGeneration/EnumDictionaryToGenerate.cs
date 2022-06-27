namespace StaticDictionaries.SourceGeneration;

public readonly struct EnumDictionaryToGenerate
{
    public EnumDictionaryToGenerate(
        string name,
        string nameSpace,
        string fullyQualifiedName,
        IReadOnlyCollection<string> propertyNames,
        Type[] propertyTypes,
        IReadOnlyCollection<EnumMemberDefinition> members,
        bool isPublic)
    {
        Name = name;
        Namespace = nameSpace;
        Members = members;
        IsPublic = isPublic;
        FullyQualifiedName = fullyQualifiedName;
        PropertyNames = propertyNames;
        PropertyTypes = propertyTypes;
    }

    public bool IsPublic { get; }

    public string Name { get; }

    public string FullyQualifiedName { get; }

    public string Namespace { get; }

    public IReadOnlyCollection<EnumMemberDefinition> Members { get; }

    public IReadOnlyCollection<string> PropertyNames { get; }

    public Type[] PropertyTypes { get; }
}