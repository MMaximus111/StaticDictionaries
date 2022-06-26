namespace Dictionaries.SourceGeneration;

public readonly struct DictionaryToGenerate
{
    public DictionaryToGenerate(
        string name,
        string nameSpace,
        string fullyQualifiedName,
        IReadOnlyCollection<DictionaryBase> properties,
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

    public IReadOnlyCollection<DictionaryBase> Properties { get; }
}