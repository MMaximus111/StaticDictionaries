using System.Reflection;

namespace Dictionaries;

internal static class Dictionaries<T>
    where T : DictionaryBase
{
    public static readonly T[] Value = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Select(x => x.GetValue(null))
        .Cast<T>()
        .OrderBy(x => x.Id)
        .ToArray();
}