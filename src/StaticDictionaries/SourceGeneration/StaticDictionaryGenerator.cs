using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace StaticDictionaries.SourceGeneration;

/// <summary>
/// Source generator for StaticDictionary enums.
/// </summary>
[Generator]
public class StaticDictionaryGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Internal source generator initialization method.
    /// </summary>
    /// <param name="context">Source generator context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumDeclarationSyntax> staticDictionaryDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsEnumActualForGeneration(s),
                transform: static (ctx, _) => GetEnumForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(staticDictionaryDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndEnums, static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsEnumActualForGeneration(SyntaxNode node) => node is EnumDeclarationSyntax { AttributeLists.Count: > 0 };

    private static EnumDeclarationSyntax? GetEnumForGeneration(GeneratorSyntaxContext context)
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

                if (fullName == AttributeConstants.StaticDictionaryAttributeFullName)
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

        List<EnumDictionaryToGenerate> dictionariesToGenerate = EnumProcessor.GetDictionariesToGenerate(compilation, distinctDictionaries, context.CancellationToken);

        if (dictionariesToGenerate.Count > 0)
        {
            StringBuilder sb = new StringBuilder();

            foreach (EnumDictionaryToGenerate dictionaryToGenerate in dictionariesToGenerate)
            {
                sb.Clear();
                string result = SourceGenerationHelper.GenerateExtensionClass(sb, dictionaryToGenerate);
                context.AddSource($"{dictionaryToGenerate.Namespace.Replace(".", "_")}_{dictionaryToGenerate.Name}" + "_DictionariesExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }
    }
}