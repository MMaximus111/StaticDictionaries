using System.Collections;
using System.Globalization;

namespace StaticDictionaries;

public abstract class StaticDictionary
{
    protected StaticDictionary(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string? Name { get; }

    public static bool operator !=(StaticDictionary x, StaticDictionary y)
    {
        return !(x == y);
    }

    public static bool operator ==(StaticDictionary x, StaticDictionary y)
    {
        return Equals(x, y);
    }

    public static T[] Items<T>()
        where T : StaticDictionary
    {
        return StaticDictionaries<T>.Value;
    }

    public static T GetById<T>(int id)
        where T : StaticDictionary
    {
        T? item = Items<T>().FirstOrDefault(x => x.Id == id);

        if (item is null)
        {
            throw new NotSupportedException($"Item with Id:{id.ToString(CultureInfo.InvariantCulture)} not supported.");
        }

        return item;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as StaticDictionary);
    }

    public bool Equals(StaticDictionary? staticDictionary)
    {
        return staticDictionary is not null && Id == staticDictionary.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"Id:{Id.ToString(CultureInfo.InvariantCulture)}, Name:{Name}";
    }
}