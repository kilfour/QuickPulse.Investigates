using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickPulse.Investigates;

public static class Is
{
    public static bool Collection(object obj) =>
        obj is IEnumerable && obj.GetType() != typeof(string);

    public static bool Dictionary(object obj)
    {
        var t = obj.GetType();
        if (typeof(IDictionary).IsAssignableFrom(t)) return true;
        return t.GetInterfaces().Any(i => i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
             i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
    }

    public static bool KeyValuePair(object obj)
    {
        var type = obj.GetType();
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
    }

    public static bool Object(object obj)
    {
        var t = obj.GetType();
        if (t == typeof(string) || obj is ITuple) return false;
        if (obj is IEnumerable) return false;
        return t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Any(p => p.CanRead && p.GetIndexParameters().Length == 0);
    }
    public static bool Tuple(object obj) => obj is ITuple;
}