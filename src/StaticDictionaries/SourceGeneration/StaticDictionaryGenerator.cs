using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using StaticDictionaries.SourceGeneration.Attributes;

namespace StaticDictionaries.SourceGeneration;

[Generator]
public class StaticDictionaryGenerator : IIncrementalGenerator
{
    private const string StaticDictionaryAttributeFullName = $"StaticDictionaries.SourceGeneration.Attributes.{StaticDictionaryAttributeName}";
    private const string StaticDictionaryAttributeName = nameof(StaticDictionaryAttribute);

    private const string ValueAttributeFullName = $"StaticDictionaries.SourceGeneration.Attributes.{ValueAttributeName}";
    private const string ValueAttributeName = nameof(ValueAttribute);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumDeclarationSyntax> staticDictionaryDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(staticDictionaryDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndEnums, static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is EnumDeclarationSyntax { AttributeLists.Count: > 0 };

    private static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        EnumDeclarationSyntax enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == StaticDictionaryAttributeFullName)
                {
                    return enumDeclarationSyntax;
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<EnumDeclarationSyntax> dictionaries, SourceProductionContext context)
    {
        if (dictionaries.IsDefaultOrEmpty)
        {
            return;
        }

        IEnumerable<EnumDeclarationSyntax> distinctDictionaries = dictionaries.Distinct();

        List<EnumDictionaryToGenerate> dictionariesToGenerate = GetDictionariesToGenerate(compilation, distinctDictionaries, context.CancellationToken);

        if (dictionariesToGenerate.Count > 0)
        {
            StringBuilder sb = new StringBuilder();

            foreach (EnumDictionaryToGenerate dictionaryToGenerate in dictionariesToGenerate)
            {
                sb.Clear();
                string result = SourceGenerationHelper.GenerateExtensionClass(sb, dictionaryToGenerate);
                context.AddSource(dictionaryToGenerate.Name + "_DictionariesExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }
    }

    private static List<EnumDictionaryToGenerate> GetDictionariesToGenerate(Compilation compilation, IEnumerable<EnumDeclarationSyntax> enums, CancellationToken ct)
    {
        List<EnumDictionaryToGenerate> dictionariesToGenerate = new List<EnumDictionaryToGenerate>();

        INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName(StaticDictionaryAttributeFullName);
        INamedTypeSymbol? enumMemberAttribute = compilation.GetTypeByMetadataName(ValueAttributeFullName);

        if (enumAttribute == null || enumMemberAttribute == null)
        {
            return dictionariesToGenerate;
        }

        foreach (EnumDeclarationSyntax enumDeclarationSyntax in enums)
        {
            ct.ThrowIfCancellationRequested();

            SemanticModel semanticModel = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
            {
                continue;
            }

            string nameSpace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString()!;

            List<string> propertyNames = new List<string>();

            foreach (AttributeData attributeData in enumSymbol.GetAttributes())
            {
                if (!enumAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                foreach (TypedConstant constructorArgument in attributeData.ConstructorArguments)
                {
                    propertyNames.AddRange(constructorArgument.Values.Select(x => x.Value.ToString()));
                }
            }

            if (propertyNames.Distinct().Count() != propertyNames.Count)
            {
                throw new ArgumentException($"Property names must not be dublicated at {enumSymbol.Name}.");
            }

            ImmutableArray<ISymbol> enumMembers = enumSymbol.GetMembers();

            List<EnumMemberDefinition> members = new List<EnumMemberDefinition>(enumMembers.Length);

            List<Type> propertyTypes = new List<Type>();

            int i = 0;

            foreach (ISymbol member in enumMembers)
            {
                if (member is not IFieldSymbol field || field.ConstantValue is null)
                {
                    continue;
                }

                List<object?> memberProperties = new List<object?>();

                foreach (AttributeData? memberAttribute in field.GetAttributes())
                {
                    if (!enumMemberAttribute.Equals(memberAttribute.AttributeClass, SymbolEqualityComparer.Default))
                    {
                        continue;
                    }

                    TypedConstant firstArgument = memberAttribute.ConstructorArguments.First();

                    if (firstArgument.Values.Count() != propertyNames.Count)
                    {
                        throw new ArgumentException($"Enum member {member.Name} has incorrect attribute parameters count at {enumSymbol.Name}.");
                    }

                    if (i == 0)
                    {
                        propertyTypes = firstArgument.Values.Select(x => x.Value?.GetType() ?? typeof(object)).ToList();
                    }

                    foreach (TypedConstant attributeArgument in firstArgument.Values)
                    {
                        memberProperties.Add(attributeArgument.Value);
                    }

                    i++;
                }

                members.Add(new EnumMemberDefinition((int)field.ConstantValue, member.Name, memberProperties.ToArray()));
            }

            dictionariesToGenerate.Add(new EnumDictionaryToGenerate(
                name: enumSymbol.Name,
                nameSpace: nameSpace,
                fullyQualifiedName: enumSymbol.Name,
                propertyNames,
                propertyTypes.ToArray(),
                members: members,
                isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public));
        }

        return dictionariesToGenerate;
    }
}