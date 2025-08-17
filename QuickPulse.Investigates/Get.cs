using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using QuickPulse;
using QuickPulse.Show;

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

    public static IEnumerable<ObjectProperty> TupleObjectProperties(Pair pair)
    {
        var left = pair.This as ITuple;
        var right = pair.That as ITuple;
        var len = Math.Max(left?.Length ?? 0, right?.Length ?? 0);
        return Enumerable.Range(0, len).Select(i => GetObjectProperty(i, left, right));
        static ObjectProperty GetObjectProperty(int i, ITuple? left, ITuple? right)
        {
            var leftValue = i < (left?.Length ?? 0) ? left?[i] : null;
            var rightValue = i < (right?.Length ?? 0) ? right?[i] : null;
            return new ObjectProperty($"[{i}]", new Pair(leftValue!, rightValue!));
        }
    }

    public static IEnumerable<(int, Pair)> IndexedPairs(Pair pair)
    {
        var L = AsSeq(pair.This).ToList();
        var R = AsSeq(pair.That).ToList();
        var len = Math.Max(L.Count, R.Count);
        return Enumerable.Range(0, len)
            .Select(i => (i, new Pair(
                i < L.Count ? L[i]! : null!,
                i < R.Count ? R[i]! : null!)));
        static IEnumerable<object?> AsSeq(object? enumerable) =>
            enumerable is null || enumerable is string
                ? [] : (enumerable as IEnumerable)?.Cast<object?>() ?? [];
    }

    public static IEnumerable<ObjectProperty> DictionaryEntries(Pair p)
    {
        var leftPairs = EnumeratePairs(p.This).ToList();
        var rightPairs = EnumeratePairs(p.That).ToList();
        var eq = EqFrom(p.This) ?? EqFrom(p.That) ?? EqualityComparer<object?>.Default;
        var seen = new HashSet<object?>(eq);
        var allKeys = new List<object?>();
        foreach (var k in leftPairs.Select(kv => kv.key)) if (seen.Add(k)) allKeys.Add(k);
        foreach (var k in rightPairs.Select(kv => kv.key)) if (seen.Add(k)) allKeys.Add(k);
        foreach (var k in allKeys)
        {
            var left = leftPairs.FirstOrDefault(kv => eq.Equals(kv.key, k));
            var right = rightPairs.FirstOrDefault(kv => eq.Equals(kv.key, k));
            var leftValue = left.exists ? left.value : null;
            var rightValue = right.exists ? right.value : null;
            yield return new ObjectProperty($"[key:{FormatKey(k)}]", new Pair(leftValue!, rightValue!));
        }

        static IEnumerable<(object? key, object? value, bool exists)> EnumeratePairs(object? dict)
        {
            if (dict is null)
                yield break;

            if (dict is IDictionary id)
            {
                foreach (DictionaryEntry e in id)
                    yield return (e.Key, e.Value, true);
                yield break;
            }

            if (dict is IEnumerable seq)
            {
                foreach (var kv in seq)
                {
                    var t = kv?.GetType();
                    var kProp = t?.GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
                    var vProp = t?.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                    if (kProp is null || vProp is null) continue;
                    yield return (kProp.GetValue(kv), vProp.GetValue(kv), true);
                }
            }
        }

        static IEqualityComparer<object?>? EqFrom(object? dict)
        {
            if (dict is null) return null;
            var prop = dict.GetType().GetProperty("Comparer", BindingFlags.Instance | BindingFlags.Public);
            if (prop?.GetValue(dict) is IEqualityComparer ic)
                return new BoxedEq(ic);
            return null;
        }

        static string FormatKey(object? key) =>
            key is string s ? $"\"{s}\"" : Introduce.This(key!, false);
    }

    private class BoxedEq : IEqualityComparer<object?>
    {
        private readonly IEqualityComparer _ic;
        public BoxedEq(IEqualityComparer ic) => _ic = ic;
        public new bool Equals(object? x, object? y) => _ic.Equals(x, y);
        public int GetHashCode(object? obj) => _ic.GetHashCode(obj!);
    }
}