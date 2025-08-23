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
        // If either side is a Set, use unordered multiset pairing that respects the set's comparer.
        if (IsSet(pair.This) || IsSet(pair.That))
            return IndexedPairsForSets(pair);

        // Otherwise: original positional behavior
        var left = AsSeq(pair.This).ToList();
        var right = AsSeq(pair.That).ToList();
        var len = Math.Max(left.Count, right.Count);
        return Enumerable.Range(0, len)
            .Select(i => (i, new Pair(
                i < left.Count ? left[i]! : null!,
                i < right.Count ? right[i]! : null!)));
    }


    private static IEnumerable<object?> AsSeq(object? enumerable) =>
        enumerable is null || enumerable is string
            ? Array.Empty<object?>()
            : (enumerable as IEnumerable)?.Cast<object?>() ?? Array.Empty<object?>();

    private static bool IsSet(object? obj) =>
        obj is not null && obj.GetType().GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));

    private static IEnumerable<(int, Pair)> IndexedPairsForSets(Pair pair)
    {
        var left = AsSeq(pair.This).ToList();
        var right = AsSeq(pair.That).ToList();

        var eq = EqFromSet(pair.This) ?? EqFromSet(pair.That) ?? EqualityComparer<object?>.Default;

        // Bucket RIGHT (non-null keys) + separate queue for nulls
        var buckets = new Dictionary<object, Queue<int>>(new NonNullObjectComparer(eq));
        var nullQueue = new Queue<int>();
        var usedRight = new bool[right.Count];

        for (int j = 0; j < right.Count; j++)
        {
            var key = right[j];
            if (key is null) { nullQueue.Enqueue(j); continue; }
            if (!buckets.TryGetValue(key, out var q)) buckets[key] = q = new Queue<int>();
            q.Enqueue(j);
        }

        var outIx = 0;

        // LEFT: if matched by set equality -> SKIP (equal by definition of the set's comparer)
        // otherwise emit (left, null)
        for (int i = 0; i < left.Count; i++)
        {
            var k = left[i];
            if (k is null)
            {
                if (nullQueue.Count > 0)
                {
                    var j = nullQueue.Dequeue();
                    usedRight[j] = true;     // matched null — no output
                }
                else
                {
                    yield return (outIx++, new Pair(null!, null!)); // "missing null" on right
                }
                continue;
            }

            if (buckets.TryGetValue(k, out var q) && q.Count > 0)
            {
                var j = q.Dequeue();
                usedRight[j] = true;         // matched — no output
            }
            else
            {
                yield return (outIx++, new Pair(left[i]!, null!));  // unmatched on right
            }
        }

        // RIGHT leftovers: emit in original RIGHT order as (null, right[j])
        for (int j = 0; j < right.Count; j++)
            if (!usedRight[j])
                yield return (outIx++, new Pair(null!, right[j]!));
    }

    private sealed class NonNullObjectComparer : IEqualityComparer<object>
    {
        private readonly IEqualityComparer<object?> _inner;
        public NonNullObjectComparer(IEqualityComparer<object?> inner) => _inner = inner;
        public new bool Equals(object? x, object? y) => _inner.Equals(x, y);
        public int GetHashCode(object obj) => _inner.GetHashCode(obj);
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

    private static IEqualityComparer<object?>? EqFromSet(object? set)
    {
        if (set is null) return null;
        var prop = set.GetType().GetProperty("Comparer", BindingFlags.Instance | BindingFlags.Public);
        var cmp = prop?.GetValue(set);
        if (cmp is null) return null;

        // Non-generic comparer
        if (cmp is IEqualityComparer nongeneric) return new BoxedEq(nongeneric);

        // Generic IEqualityComparer<T> -> box it
        var iface = cmp.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEqualityComparer<>));
        if (iface is not null)
        {
            var tArg = iface.GetGenericArguments()[0];
            return new BoxedGenericEq(cmp, tArg);
        }
        return null;
    }

    private sealed class BoxedGenericEq : IEqualityComparer<object?>
    {
        private readonly object _cmp;
        private readonly Type _t;
        private readonly MethodInfo _equals;
        private readonly MethodInfo _getHashCode;

        public BoxedGenericEq(object cmp, Type t)
        {
            _cmp = cmp;
            _t = t;
            _equals = cmp.GetType().GetMethod("Equals", new[] { t, t })!;
            _getHashCode = cmp.GetType().GetMethod("GetHashCode", new[] { t })!;
        }

        public new bool Equals(object? x, object? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            if (!_t.IsInstanceOfType(x) || !_t.IsInstanceOfType(y))
                return EqualityComparer<object?>.Default.Equals(x, y);
            return (bool)_equals.Invoke(_cmp, new[] { x, y })!;
        }

        public int GetHashCode(object? obj)
        {
            if (obj is null) return 0;
            if (!_t.IsInstanceOfType(obj))
                return EqualityComparer<object?>.Default.GetHashCode(obj);
            return (int)_getHashCode.Invoke(_cmp, new[] { obj })!;
        }
    }

    private class BoxedEq : IEqualityComparer<object?>
    {
        private readonly IEqualityComparer _ic;
        public BoxedEq(IEqualityComparer ic) => _ic = ic;
        public new bool Equals(object? x, object? y) => _ic.Equals(x, y);
        public int GetHashCode(object? obj) => _ic.GetHashCode(obj!);
    }
}