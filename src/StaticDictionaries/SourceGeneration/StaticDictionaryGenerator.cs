using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace StaticDictionaries.SourceGeneration;

[Generator]
public class StaticDictionaryGenerator : IIncrementalGenerator
{
    private const string DisplayAttribute = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string AttributeFullName = $"StaticDictionaries.SourceGeneration.{AttributeName}";
    private const string AttributeName = nameof(SourceGenerationAttribute);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
        //     $"{AttributeName}.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<ClassDeclarationSyntax> staticDictionaryDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(staticDictionaryDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndEnums, static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        ClassDeclarationSyntax classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == AttributeFullName)
                {
                    return classDeclarationSyntax;
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> dictionaries, SourceProductionContext context)
    {
        if (dictionaries.IsDefaultOrEmpty)
        {
            return;
        }

        IEnumerable<ClassDeclarationSyntax> distinctDictionaries = dictionaries.Distinct();

        List<StaticDictionaryToGenerate> dictionariesToGenerate = GetDictionariesToGenerate(compilation, distinctDictionaries, context.CancellationToken);

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

    private static List<StaticDictionaryToGenerate> GetDictionariesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> enums, CancellationToken ct)
    {
        List<StaticDictionaryToGenerate> dictionariesToGenerate = new List<StaticDictionaryToGenerate>();

        INamedTypeSymbol? classAttribute = compilation.GetTypeByMetadataName(AttributeFullName);

        if (classAttribute == null)
        {
            return dictionariesToGenerate;
        }

        INamedTypeSymbol? displayAttribute = compilation.GetTypeByMetadataName(DisplayAttribute);

        foreach (ClassDeclarationSyntax classDeclarationSyntax in enums)
        {
            ct.ThrowIfCancellationRequested();

            SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            {
                continue;
            }

            string name = classSymbol.Name + "Extensions";
            string nameSpace = classSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : classSymbol.ContainingNamespace.ToString()!;

            foreach (AttributeData attributeData in classSymbol.GetAttributes())
            {
                if (!classAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
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

                    if (namedArgument.Key == "ExtensionClassName" && namedArgument.Value.Value?.ToString() is { } n)
                    {
                        name = n;
                    }
                }
            }

            string fullyQualifiedName = classSymbol.ToString()!;

            ImmutableArray<ISymbol> classMembers = classSymbol.GetMembers();

            List<StaticDictionary> properties = new List<StaticDictionary>(classMembers.Length);

            HashSet<string> displayNames = new HashSet<string>();

            bool isDisplayNameTheFirstPresence = false;

            foreach (ISymbol member in classMembers)
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

                        foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
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

                properties.Add(null);
            }

            dictionariesToGenerate.Add(new StaticDictionaryToGenerate(
                name: name,
                nameSpace: nameSpace,
                fullyQualifiedName: fullyQualifiedName,
                properties: properties,
                isPublic: classSymbol.DeclaredAccessibility == Accessibility.Public));
        }

        return dictionariesToGenerate;
    }
}