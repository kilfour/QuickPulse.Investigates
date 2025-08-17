using System.Reflection;
using QuickPulse;

namespace QuickPulse.Investigates;

public static class Get
{
    public static IEnumerable<PropertyInfo> Properties(object input) =>
        input.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public);

    public static IEnumerable<FieldInfo> Fields(object input) =>
        input.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public);

    public static IEnumerable<ObjectProperty> ObjectProperties(Pair pair) =>
        Properties(pair.This).Select(
            a => new ObjectProperty(a.Name, new Pair(a.GetValue(pair.This)!, a.GetValue(pair.That)!)))
                .Union(Fields(pair.This).Select(
            a => new ObjectProperty(a.Name, new Pair(a.GetValue(pair.This)!, a.GetValue(pair.That)!))));

    public static IEnumerable<object> FieldValues(object input) =>
        input.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(a => a.GetValue(input)!);

    public static (Pair, Pair) KeyValueAsLabeledPair(Pair pair)
    {
        return new(new Pair(
             pair.This.GetType().GetProperty("Key")?.GetValue(pair.This)!,
             pair.This.GetType().GetProperty("Key")?.GetValue(pair.That)!),
            new Pair(
                pair.This.GetType().GetProperty("Value")?.GetValue(pair.This)!,
                pair.This.GetType().GetProperty("Value")?.GetValue(pair.That)!));
    }
}