namespace StaticDictionaries.SourceGeneration;

public readonly struct StaticDictionaryToGenerate
{
    public StaticDictionaryToGenerate(
        string name,
        string nameSpace,
        string fullyQualifiedName,
        IReadOnlyCollection<StaticDictionary> properties,
        bool isPublic)
    {
        Name = name;
        Namespace = nameSpace;
        Properties = properties;
        IsPublic = isPublic;
        FullyQualifiedName = fullyQualifiedName;
    }

    public bool IsPublic { get; }

    public string Name { get; }

    public string FullyQualifiedName { get; }

    public string Namespace { get; }

    public IReadOnlyCollection<StaticDictionary> Properties { get; }
}