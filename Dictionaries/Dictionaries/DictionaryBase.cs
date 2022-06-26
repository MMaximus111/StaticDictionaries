using System.Globalization;

namespace Dictionaries;

public abstract class DictionaryBase
{
    protected DictionaryBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string? Name { get; }

    public static bool operator !=(DictionaryBase x, DictionaryBase y)
    {
        return !(x == y);
    }

    public static bool operator ==(DictionaryBase x, DictionaryBase y)
    {
        return Equals(x, y);
    }

    public static T[] Items<T>()
        where T : DictionaryBase
    {
        return Dictionaries<T>.Value;
    }

    public static T GetById<T>(int id)
        where T : DictionaryBase
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
        return Equals(obj as DictionaryBase);
    }

    public bool Equals(DictionaryBase? dictionaryItem)
    {
        return dictionaryItem is not null && Id == dictionaryItem.Id;
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