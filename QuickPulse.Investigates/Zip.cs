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

    public static IEnumerable<ObjectProperty> DictionaryEntries(Pair p)
    {
        var leftPairs = EnumeratePairs(p.This).ToList();
        var rightPairs = EnumeratePairs(p.That).ToList();

        var eq = EqFrom(p.This) ?? EqFrom(p.That) ?? EqualityComparer<object?>.Default;

        // Build deterministic union without sorting (avoids heterogeneous key compare errors)
        var seen = new HashSet<object?>(eq);
        var allKeys = new List<object?>();
        foreach (var k in leftPairs.Select(kv => kv.key)) if (seen.Add(k)) allKeys.Add(k);
        foreach (var k in rightPairs.Select(kv => kv.key)) if (seen.Add(k)) allKeys.Add(k);

        foreach (var k in allKeys)
        {
            var l = leftPairs.FirstOrDefault(kv => eq.Equals(kv.key, k));
            var r = rightPairs.FirstOrDefault(kv => eq.Equals(kv.key, k));

            var lv = l.exists ? l.value : null;
            var rv = r.exists ? r.value : null;

            yield return new ObjectProperty($"[key:{FormatKey(k)}]", new Pair(lv!, rv!));
        }

        // --- helpers ---
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
    }

    static string FormatKey(object? k)
        => k is string s ? $"\"{s}\"" : Introduce.This(k!, false);

    sealed class BoxedEq : IEqualityComparer<object?>
    {
        private readonly IEqualityComparer _ic;
        public BoxedEq(IEqualityComparer ic) => _ic = ic;
        public new bool Equals(object? x, object? y) => _ic.Equals(x, y);
        public int GetHashCode(object? obj) => _ic.GetHashCode(obj!);
    }

}