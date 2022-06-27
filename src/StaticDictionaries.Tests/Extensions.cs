using StaticDictionaries.Tests.StaticDictionaries;

namespace StaticDictionaries.Tests;

public static class Extensions
{
    public static string Name(this Employee employee)
    {
        return employee switch
        {
            Employee.Maxim => "Максим",
            Employee.John => "Джон",
            _ => throw new ArgumentOutOfRangeException(nameof(employee), employee, null)
        };
    }
}