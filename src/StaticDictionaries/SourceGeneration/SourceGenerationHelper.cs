﻿using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace StaticDictionaries.SourceGeneration;

internal static class SourceGenerationHelper
{
    private const string IdPropertyName = "Id";
    private const string NamePropertyName = "Name";

    private const string Header = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the StaticDictionaries source generator
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable";

    public static string GenerateExtensionClass(StringBuilder sb, EnumDictionaryToGenerate dictionaryToGenerate)
    {
        sb
            .Append(Header)
            .Append(@"
using System;

");
        if (!string.IsNullOrEmpty(dictionaryToGenerate.Namespace))
        {
            sb.Append(@"
namespace ").Append(dictionaryToGenerate.Namespace).Append(@"
{");
        }

        sb.Append(@"
    ").Append(dictionaryToGenerate.IsPublic ? "public" : "internal").Append(@" static partial class ").Append($"{dictionaryToGenerate.Name}Extensions").Append(@"
    {
        /// <summary>
        /// The number of members in the enum.
        /// This is a non-distinct count of defined names.
        /// </summary>
        public const int Length = ").Append(dictionaryToGenerate.Members.Count).Append(";");

        sb.AppendLine();

        List<string> propertyNames = dictionaryToGenerate.PropertyNames.ToList();

        bool idPropertyDefined = dictionaryToGenerate.PropertyNames.Contains(IdPropertyName);
        bool namePropertyDefined = dictionaryToGenerate.PropertyNames.Contains(NamePropertyName);

        if (!idPropertyDefined)
        {
            propertyNames.Add(IdPropertyName);

            dictionaryToGenerate.PropertyTypes.Add(typeof(int));

            foreach (EnumMemberDefinition member in dictionaryToGenerate.Members)
            {
                member.Values.Add(member.Id);
            }
        }

        if (!namePropertyDefined)
        {
            dictionaryToGenerate.PropertyTypes.Add(typeof(string));

            propertyNames.Add(NamePropertyName);

            foreach (EnumMemberDefinition member in dictionaryToGenerate.Members)
            {
                member.Values.Add(member.MemberName);
            }
        }

        sb.Append(@"/// <summary>
    /// Max id of enum.
    /// Returns identifier of member that cannot be overridden by attribute arguments.
    /// </summary>
    public const int MaxId = ").Append(dictionaryToGenerate.Members.Max(x => x.Id)).Append(";");

        sb.AppendLine();

        sb.Append(@"/// <summary>
    /// Min id of enum.
    /// Returns identifier of member that cannot be overridden by attribute arguments.
    /// </summary>
    public const int MinId = ").Append(dictionaryToGenerate.Members.Min(x => x.Id)).Append(";");

        sb.AppendLine();

        int i = 0;
        foreach (string propertyName in propertyNames)
        {
            Type type = dictionaryToGenerate.PropertyTypes[i];

            sb.AppendLine(@"
        /// <summary>
        /// Generated method by StaticDictionaries.
        /// </summary>
        /// <returns>Value for defined property.</returns>");

            sb.AppendLine(@$"public static {type.Name} {propertyName}(this {dictionaryToGenerate.Name} member)");
            sb.Append("{");
            sb.AppendLine();

            foreach ((string MemberName, object value) property in dictionaryToGenerate.Members.Select(x => (x.MemberName, x.Values[i]!)))
            {
                sb.AppendLine($"if (member == {dictionaryToGenerate.Name}.{property.MemberName})");
                sb.AppendLine($"return {ToLiteral(property.value)};");
            }

            sb.AppendLine("return default;");
            sb.AppendLine("}");
            sb.AppendLine();

            i++;
        }

        sb.AppendLine();

        sb.AppendLine(@"
        /// <summary>
        /// Generated method by StaticDictionaries.
        /// </summary>
        /// <returns>Enum member by id.</returns>");
        sb.AppendLine(@$"public static {dictionaryToGenerate.Name} GetById(int id)");
        sb.AppendLine("{");

        int idPropertyPosition = propertyNames.IndexOf(IdPropertyName);

        foreach (EnumMemberDefinition? member in dictionaryToGenerate.Members)
        {
            sb.AppendLine($"if (id == {member.Values[idPropertyPosition]})");
            sb.AppendLine($"return {dictionaryToGenerate.Name}.{member.MemberName};");
        }

        sb.AppendLine("return default;");

        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine(@"
        /// <summary>
        /// Generated method by StaticDictionaries.
        /// </summary>
        /// <returns>Array of all enum members.</returns>");
        sb.AppendLine($@"public static {dictionaryToGenerate.Name}[] All()");
        sb.AppendLine("{");
        sb.AppendLine("return new[]");
        sb.AppendLine("{");

        foreach (EnumMemberDefinition? member in dictionaryToGenerate.Members)
        {
            sb.Append(dictionaryToGenerate.Name).Append('.').Append(member.MemberName).Append(",");
            sb.AppendLine();
        }

        sb.AppendLine("};");
        sb.Append("}");

        sb.Append(@"
    }");
        if (!string.IsNullOrEmpty(dictionaryToGenerate.Namespace))
        {
            sb.Append(@"
}");
        }

        return sb.ToString();
    }

    private static string ToLiteral(object value)
    {
        return SymbolDisplay.FormatPrimitive(value, true, false);
    }
}