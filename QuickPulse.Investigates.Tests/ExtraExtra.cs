namespace QuickPulse.Investigates.Tests;

public class ExtraExtra
{
    [Fact]
    public void Dict_case_insensitive_respected()
    {
        var a = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["Id"] = 1 };
        var b = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["id"] = 2 };
        var f = Investigate.These(a, b);
        Assert.False(f.AllEqual);
        Assert.Equal("[key:\"Id\"]: 1 /= 2", f.Report); // or \"id\" depending on your formatting
    }

    [Fact]
    public void Hashtable_supported()
    {
        var a = new System.Collections.Hashtable { ["k"] = 1 };
        var b = new System.Collections.Hashtable { ["k"] = 2 };
        var f = Investigate.These(a, b);
        Assert.False(f.AllEqual);
        Assert.Equal("[key:\"k\"]: 1 /= 2", f.Report);
    }

    class Throwy { public int Ok => 1; public int Bad => throw new InvalidOperationException(); }

    [Fact(Skip = "Not implemented")]
    public void Throwing_getter_reports()
    {
        var f = Investigate.These(new Throwy(), new Throwy());
        Assert.False(f.AllEqual);
        Assert.Contains("Bad: accessor threw", f.Report);
    }
    // static (object? v, bool err) TryGet(Func<object?> f)
    // {
    // try { return (f(), false); } catch { return (null, true); }
    // }
    // // when reading a prop:
    // var (lv, le) = TryGet(() => p.GetValue(l));
    // var (rv, re) = TryGet(() => p.GetValue(r));
    // if (le || re) Trace($"{path}.{p.Name}: accessor threw");

}
