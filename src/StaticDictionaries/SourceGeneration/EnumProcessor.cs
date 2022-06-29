using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticDictionaries.SourceGeneration;

internal static class EnumProcessor
{
    public static List<EnumDictionaryToGenerate> GetDictionariesToGenerate(Compilation compilation, IEnumerable<EnumDeclarationSyntax> enums, CancellationToken ct)
    {
        List<EnumDictionaryToGenerate> dictionariesToGenerate = new List<EnumDictionaryToGenerate>();

        INamedTypeSymbol? staticDictionaryAttribute = compilation.GetTypeByMetadataName(AttributeConstants.StaticDictionaryAttributeFullName);
        INamedTypeSymbol? valueAttribute = compilation.GetTypeByMetadataName(AttributeConstants.ValueAttributeFullName);

        if (staticDictionaryAttribute == null || valueAttribute == null)
        {
            return dictionariesToGenerate;
        }

        Regex propertyNameRegex = new Regex("^[A-Za-z\\d]+$");

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
                if (!staticDictionaryAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                TypedConstant firstArgument = attributeData.ConstructorArguments[0];
                TypedConstant secondParamsArgument = attributeData.ConstructorArguments[1];

                propertyNames.Add(firstArgument.Value!.ToString());
                propertyNames.AddRange(secondParamsArgument.Values.Select(x => x.Value!.ToString()));
            }

            if (propertyNames.Distinct().Count() != propertyNames.Count)
            {
                throw new ArgumentException($"Property names must not be dublicated at {enumSymbol.Name}.");
            }

            foreach (string? propertyName in propertyNames)
            {
                if (!propertyNameRegex.IsMatch(propertyName))
                {
                    throw new ArgumentException($"Property name {propertyName} has incorrect name for generation at {enumSymbol.Name}.");
                }
            }

            ImmutableArray<ISymbol> enumMembers = enumSymbol.GetMembers();

            if (!enumMembers.Any())
            {
                continue;
            }

            List<EnumMemberDefinition> membersWithValueAttribute = new List<EnumMemberDefinition>(enumMembers.Length);

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
                    if (!valueAttribute.Equals(memberAttribute.AttributeClass, SymbolEqualityComparer.Default))
                    {
                        continue;
                    }

                    TypedConstant firstArgument = memberAttribute.ConstructorArguments.First();

                    if (firstArgument.Values.Count() != propertyNames.Count)
                    {
                        throw new ArgumentException($"`StaticDictionary` enum member {member.Name} has incorrect attribute parameters count at {enumSymbol.Name}.\n Expecting: {propertyNames.Count}, actual: {firstArgument.Values.Count()}");
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

                membersWithValueAttribute.Add(new EnumMemberDefinition((int)field.ConstantValue, member.Name, memberProperties.ToArray()));
            }

            if (!membersWithValueAttribute.Any())
            {
                continue;
            }

            if (membersWithValueAttribute.Count != enumMembers.Count(x => x is IFieldSymbol { ConstantValue: { } }))
            {
                throw new ArgumentException($"All `StaticDictionary` enum members must have `Value` attribyte. At {enumSymbol.Name}.");
            }

            dictionariesToGenerate.Add(new EnumDictionaryToGenerate(
                name: enumSymbol.Name,
                nameSpace: nameSpace,
                propertyNames,
                propertyTypes.ToArray(),
                members: membersWithValueAttribute,
                isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public));
        }

        return dictionariesToGenerate;
    }
}