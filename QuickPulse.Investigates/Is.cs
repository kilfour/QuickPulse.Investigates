using System.Collections;
using System.Runtime.CompilerServices;

namespace QuickPulse.Investigates;

public static class Is
{
    // public static bool Primitive(object obj, PrimitivesRegistry registry)
    // {
    //     return registry.HasType(obj.GetType());
    // }

    public static bool Collection(object obj)
    {
        return obj is IEnumerable && obj.GetType() != typeof(string);
    }

    public static bool Dictionary(object obj)
    {
        return obj.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    public static bool KeyValuePair(object obj)
    {
        var type = obj.GetType();
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
    }

    public static bool Object(object obj)
    {
        var type = obj.GetType();
        if (type == typeof(string)) return false;
        return type.IsClass;
    }
    public static bool Tuple(object obj)
    {
        return obj is ITuple;
        // return obj.GetType().FullName!.StartsWith("System.ValueTuple");
    }
}