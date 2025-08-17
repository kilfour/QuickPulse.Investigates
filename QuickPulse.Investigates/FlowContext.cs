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

    private readonly HashSet<object> visited = new(ReferenceEqualityComparer.Instance);
    public bool AlreadyVisited(object obj)
    {
        if (obj == null || obj.GetType().IsValueType) return false; // just to be on the safe side
        if (visited.Contains(obj)) return true;
        visited.Add(obj);
        return false;
    }
}