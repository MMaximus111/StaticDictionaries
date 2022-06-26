namespace StaticDictionaries.SourceGeneration;

[AttributeUsage(AttributeTargets.Class)]
public class SourceGenerationAttribute : System.Attribute
{
    /// <summary>
    /// The namespace to generate the extension class.
    /// If not provided the namespace of the class will be used
    /// </summary>
    public string? ExtensionClassNamespace { get; set; }

    /// <summary>
    /// The name to use for the extension class.
    /// If not provided, the enum name with ""Extensions"" will be used.
    /// For example for a static dictionary called StatusCodes, the default name
    /// will be StatusCodesExtensions
    /// </summary>
    public string? ExtensionClassName { get; set; }
}