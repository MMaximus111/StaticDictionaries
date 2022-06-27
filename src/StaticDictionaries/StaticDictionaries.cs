// using System.Reflection;
//
// namespace StaticDictionaries;
//
// internal static class StaticDictionaries<T>
//     where T : StaticDictionary
// {
//     public static readonly T[] Value = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Static)
//         .Select(x => x.GetValue(null))
//         .Cast<T>()
//         .OrderBy(x => x.Id)
//         .ToArray();
// }