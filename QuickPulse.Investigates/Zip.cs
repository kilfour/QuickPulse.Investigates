using System.Collections;
using System.Reflection;
using QuickPulse.Show;

namespace QuickPulse.Investigates;

public static class Zip
{
    public static IEnumerable<(int, Pair)> ToIndexedPairs(Pair pair)
    {
        var L = AsSeq(pair.This).ToList();
        var R = AsSeq(pair.That).ToList();
        var len = Math.Max(L.Count, R.Count);

        return Enumerable.Range(0, len)
            .Select(i => (i, new Pair(
                i < L.Count ? L[i]! : null!,
                i < R.Count ? R[i]! : null!)));
    }
    private static IEnumerable<object?> AsSeq(object? enumerable) =>
        enumerable is null || enumerable is string
            ? [] : (enumerable as IEnumerable)?.Cast<object?>() ?? [];

    public static IEnumerable<ObjectProperty> DictionaryEntries(Pair pair)
    {
        var left = ToMap(pair.This);
        var right = ToMap(pair.That);

        var allKeys = left.Keys.Concat(right.Keys)
                            .Distinct(EqualityComparer<object?>.Default)
                            .OrderBy(k => k);
        foreach (var key in allKeys)
        {
            left.TryGetValue(key, out var leftValue);
            right.TryGetValue(key, out var rightValue);
            yield return new ObjectProperty($"[key:{FormatKey(key)}]", new Pair(leftValue!, rightValue!));
        }
    }

    private static IDictionary<object?, object?> ToMap(object? dict)
    {
        var map = new Dictionary<object, object?>(EqualityComparer<object?>.Default);
        if (dict is null) return map!;

        if (dict is IDictionary id)
        {
            foreach (DictionaryEntry e in id) map[e.Key] = e.Value;
            return map!;
        }

        if (dict is IEnumerable seq)
        {
            foreach (var kv in seq)
            {
                var t = kv?.GetType();
                var k = t?.GetProperty("Key", BindingFlags.Instance | BindingFlags.Public)?.GetValue(kv);
                var v = t?.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(kv);
                if (t is not null) map[k!] = v;
            }
        }
        return map!;
    }

    static string FormatKey(object? k)
        => k is string s ? $"\"{s}\"" : Introduce.This(k!, false);
}