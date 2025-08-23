using System.Runtime.CompilerServices;

namespace QuickPulse.Investigates;

public record FlowContext
{
    public FlowContext() { }
    public string Trace { get; init; } = string.Empty;
    public string GetTracePrefix() => Trace == string.Empty ? string.Empty : Trace + ": ";
    public FlowContext AddTrace(string trace) =>
        this with { Trace = Trace == string.Empty ? trace : Trace + "." + trace };

    public FlowContext AddKey(string trace) =>
        this with { Trace = Trace + trace };
    public FlowContext AddIndex(int index) =>
        this with { Trace = Trace + "[" + index + "]" };

    private readonly HashSet<RefPair> visited = new(RefPair.ByRef.Instance);

    public bool AlreadyVisited(Pair pair)
    {
        if (pair.This is null || pair.That is null) return false;
        if (pair.This.GetType().IsValueType || pair.That.GetType().IsValueType) return false;

        return !visited.Add(new RefPair(pair.This, pair.That));
    }

    private readonly struct RefPair(object l, object r)
    {
        public readonly object L = l;
        public readonly object R = r;

        public sealed class ByRef : IEqualityComparer<RefPair>
        {
            public static readonly ByRef Instance = new();
            public bool Equals(RefPair x, RefPair y)
                => ReferenceEquals(x.L, y.L) && ReferenceEquals(x.R, y.R);

            public int GetHashCode(RefPair p)
                => (RuntimeHelpers.GetHashCode(p.L) * 397)
                 ^ RuntimeHelpers.GetHashCode(p.R);
        }
    }
}