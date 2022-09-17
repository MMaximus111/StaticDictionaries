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

        INamedTypeSymbol staticDictionaryAttribute = compilation.GetTypeByMetadataName(AttributeConstants.StaticDictionaryAttributeFullName)!;
        INamedTypeSymbol valueAttribute = compilation.GetTypeByMetadataName(AttributeConstants.ValueAttributeFullName)!;
        INamedTypeSymbol jsonSupportAttribute = compilation.GetTypeByMetadataName(AttributeConstants.JsonSupportAttributeFullName)!;
        INamedTypeSymbol xmlSupportAttribute = compilation.GetTypeByMetadataName(AttributeConstants.XmlSupportAttributeFullName)!;

        Regex propertyNameRegex = new Regex("^[A-Za-z\\d]+$");

        foreach (EnumDeclarationSyntax enumDeclarationSyntax in enums)
        {
            ct.ThrowIfCancellationRequested();

            EnumDictionaryToGenerate? processedEnum = ProcessEnum(
                compilation,
                enumDeclarationSyntax,
                propertyNameRegex,
                staticDictionaryAttribute,
                valueAttribute,
                jsonSupportAttribute,
                xmlSupportAttribute);

            if (processedEnum is not null)
            {
                dictionariesToGenerate.Add(processedEnum);
            }
        }

        return dictionariesToGenerate;
    }

    private static EnumDictionaryToGenerate? ProcessEnum(
        Compilation compilation,
        SyntaxNode enumDeclarationSyntax,
        Regex propertyNameRegex,
        ISymbol staticDictionaryAttribute,
        ISymbol valueAttribute,
        ISymbol jsonSupportAttribute,
        ISymbol xmlSupportAttribute)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);

        INamedTypeSymbol enumSymbol = (semanticModel.GetDeclaredSymbol(enumDeclarationSyntax) as INamedTypeSymbol)!;

        string nameSpace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString()!;

        List<string> propertyNames = new List<string>();

        foreach (AttributeData attributeData in enumSymbol.GetAttributes())
        {
            ProcessEnumAttribute(attributeData, staticDictionaryAttribute, propertyNames);
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

        List<EnumMemberDefinition> membersWithValueAttribute = new List<EnumMemberDefinition>(enumMembers.Length);

        List<Type> propertyTypes = new List<Type>();

        bool firstMember = true;

        foreach (ISymbol member in enumMembers)
        {
            if (member is not IFieldSymbol field || field.ConstantValue is null)
            {
                continue;
            }

            List<object?> memberProperties = new List<object?>();

            bool containsValueAttribute = false;

            foreach (AttributeData? memberAttribute in field.GetAttributes())
            {
                if (!valueAttribute.Equals(memberAttribute.AttributeClass, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                containsValueAttribute = true;

                TypedConstant firstArgument = memberAttribute.ConstructorArguments.First();

                if (firstArgument.Values.Count() != propertyNames.Count)
                {
                    throw new ArgumentException($"`StaticDictionary` enum member {member.Name} has incorrect attribute arguments count at {enumSymbol.Name}.\n Expecting: {propertyNames.Count}, actual: {firstArgument.Values.Count()}");
                }

                if (firstMember)
                {
                    propertyTypes = firstArgument.Values.Select(x => x.Value?.GetType() ?? typeof(object)).ToList();

                    firstMember = false;
                }

                int argumentPosition = 0;
                foreach (TypedConstant attributeArgument in firstArgument.Values)
                {
                    Type argumentType = attributeArgument.Value?.GetType() ?? typeof(object);
                    Type expectedType = propertyTypes[argumentPosition];

                    // logic inside statement allows using int and double types in the same position arguments
                    if (!firstMember)
                    {
                        if (argumentType == typeof(double) && expectedType == typeof(int))
                        {
                            expectedType = typeof(double);
                            propertyTypes[argumentPosition] = typeof(double);
                        }

                        if (argumentType == typeof(int) && expectedType == typeof(double))
                        {
                            argumentType = typeof(double);
                        }
                    }

                    if (argumentType != expectedType)
                    {
                        throw new ArgumentException($"`StaticDictionary` enum member {member.Name} has incorrect attribute parameter type. Enum: {enumSymbol.Name}. EnumMember: {member.Name}. Parameter: {attributeArgument.Value}");
                    }

                    memberProperties.Add(attributeArgument.Value);
                    argumentPosition++;
                }
            }

            if (containsValueAttribute)
            {
                membersWithValueAttribute.Add(new EnumMemberDefinition((int)field.ConstantValue, member.Name, memberProperties.ToList()));
            }
        }

        if (!membersWithValueAttribute.Any())
        {
            return null;
        }

        IReadOnlyCollection<INamedTypeSymbol> enumAttributtes = enumSymbol.GetAttributes().Select(x => x.AttributeClass!).ToArray();

        bool jsonSupport = enumAttributtes.Any(x => x.Equals(jsonSupportAttribute, SymbolEqualityComparer.Default));
        bool xmlSupport = enumAttributtes.Any(x => x.Equals(xmlSupportAttribute, SymbolEqualityComparer.Default));

        return new EnumDictionaryToGenerate(
            name: enumSymbol.Name,
            nameSpace: nameSpace,
            propertyNames,
            propertyTypes.ToList(),
            members: membersWithValueAttribute,
            isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public,
            jsonSupport: jsonSupport,
            xmlSupport: xmlSupport);
    }

    private static void ProcessEnumAttribute(
        AttributeData attributeData,
        ISymbol staticDictionaryAttribute,
        List<string> propertyNames)
    {
        if (!staticDictionaryAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
        {
            return;
        }

        TypedConstant argumentsArray = attributeData.ConstructorArguments[0];

        propertyNames.AddRange(argumentsArray.Values.Select(x => x.Value!.ToString()));
    }
}