namespace StaticDictionaries.SourceGeneration;

internal sealed class EnumDictionaryToGenerate
{
    public EnumDictionaryToGenerate(
        string name,
        string nameSpace,
        IReadOnlyCollection<string> propertyNames,
        List<Type> propertyTypes,
        IReadOnlyCollection<EnumMemberDefinition> members,
        bool isPublic,
        bool xmlSupport,
        bool jsonSupport)
    {
        Name = name;
        Namespace = nameSpace;
        Members = members;
        IsPublic = isPublic;
        XmlSupport = xmlSupport;
        JsonSupport = jsonSupport;
        PropertyNames = propertyNames;
        PropertyTypes = propertyTypes;
    }

    public bool IsPublic { get; }

    public bool XmlSupport { get; }

    public bool JsonSupport { get; }

    public string Name { get; }

    public string Namespace { get; }

    public IReadOnlyCollection<EnumMemberDefinition> Members { get; }

    public IReadOnlyCollection<string> PropertyNames { get; }

    public List<Type> PropertyTypes { get; }
}