namespace QuickPulse.Investigates;

public class Options
{
    private readonly Dictionary<Type, object> sortKeys = new();

    public Options Sorting<TElement>(Func<TElement, IComparable> key)
    {
        sortKeys[typeof(TElement)] = key;
        return this;
    }

    public bool TryGetSortKey(Type elementType, out object key)
        => sortKeys.TryGetValue(elementType, out key!);

    public static Options Default { get; } = new();
}

