using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dictionaries.SourceGeneration;

[Generator]
public class DictionaryGenerator : IIncrementalGenerator
{
    private const string DisplayAttribute = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string EnumExtensionsAttribute = "NetEscapades.EnumGenerators.EnumExtensionsAttribute";
    private const string HasFlagsAttribute = "System.HasFlagsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(enumDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is EnumDeclarationSyntax m && m.AttributeLists.Count > 0;

    static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName == EnumExtensionsAttribute)
                {
                    // return the enum
                    return enumDeclarationSyntax;
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> dictionaries, SourceProductionContext context)
    {
        if (dictionaries.IsDefaultOrEmpty)
        {
            return;
        }

        IEnumerable<ClassDeclarationSyntax> distinctDictionaries = dictionaries.Distinct();

        List<DictionaryToGenerate> dictionariesToGenerate = GetDictionariesToGenerate(compilation, distinctDictionaries, context.CancellationToken);
        if (dictionariesToGenerate.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var dictionaryToGenerate in dictionariesToGenerate)
            {
                sb.Clear();
                string result = SourceGenerationHelper.GenerateExtensionClass(sb, dictionaryToGenerate);
                context.AddSource(dictionaryToGenerate.Name + "_DictionariesExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }
    }

    static List<DictionaryToGenerate> GetDictionariesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> enums, CancellationToken ct)
    {
        List<DictionaryToGenerate> enumsToGenerate = new List<DictionaryToGenerate>();
        INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName(EnumExtensionsAttribute);
        if (enumAttribute == null)
        {
            // nothing to do if this type isn't available
            return enumsToGenerate;
        }

        INamedTypeSymbol? displayAttribute = compilation.GetTypeByMetadataName(DisplayAttribute);
        INamedTypeSymbol? hasFlagsAttribute = compilation.GetTypeByMetadataName(HasFlagsAttribute);

        foreach (ClassDeclarationSyntax classDeclarationSyntax in enums)
        {
            // stop if we're asked to
            ct.ThrowIfCancellationRequested();

            SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            {
                // report diagnostic, something went wrong
                continue;
            }

            string name = classSymbol.Name + "Extensions";
            string? nameSpace = classSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : classSymbol.ContainingNamespace.ToString();

            foreach (AttributeData attributeData in classSymbol.GetAttributes())
            {
                if (!enumAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                foreach (KeyValuePair<string, TypedConstant> namedArgument in attributeData.NamedArguments)
                {
                    if (namedArgument.Key == "ExtensionClassNamespace" && namedArgument.Value.Value?.ToString() is { } ns)
                    {
                        nameSpace = ns;
                        continue;
                    }

                    if (namedArgument.Key == "ExtensionClassName"
                        && namedArgument.Value.Value?.ToString() is { } n)
                    {
                        name = n;
                    }
                }
            }

            string? fullyQualifiedName = classSymbol.ToString();
            string underlyingType = classSymbol.EnumUnderlyingType?.ToString() ?? "int";

            ImmutableArray<ISymbol> enumMembers = classSymbol.GetMembers();
            List<DictionaryBase> properties = new List<DictionaryBase>(enumMembers.Length);
            HashSet<string> displayNames = new HashSet<string>();
            bool isDisplayNameTheFirstPresence = false;

            foreach (ISymbol member in enumMembers)
            {
                if (member is not IFieldSymbol field || field.ConstantValue is null)
                {
                    continue;
                }

                string? displayName = null;
                if (displayAttribute is not null)
                {
                    foreach (var attribute in member.GetAttributes())
                    {
                        if(!displayAttribute.Equals(attribute.AttributeClass, SymbolEqualityComparer.Default))
                        {
                            continue;
                        }

                        foreach (var namedArgument in attribute.NamedArguments)
                        {
                            if (namedArgument.Key == "Name" && namedArgument.Value.Value?.ToString() is { } dn)
                            {
                                displayName = dn;
                                isDisplayNameTheFirstPresence = displayNames.Add(displayName);
                                break;
                            }
                        }
                    }
                }

                properties.Add(new (member.Name, new EnumValueOption(displayName, isDisplayNameTheFirstPresence)));
            }

            enumsToGenerate.Add(new DictionaryToGenerate(
                name: name,
                fullyQualifiedName: fullyQualifiedName,
                nameSpace: nameSpace,
                isPublic: classSymbol.DeclaredAccessibility == Accessibility.Public,
                properties: properties));
        }

        return enumsToGenerate;
    }

    static string GetNamespace(EnumDeclarationSyntax enumDeclarationSyntax)
    {
        // determine the namespace the class is declared in, if any
        string nameSpace = string.Empty;
        SyntaxNode? potentialNamespaceParent = enumDeclarationSyntax.Parent;
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                namespaceParent = parent;
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
            }
        }

        return nameSpace;
    }
}