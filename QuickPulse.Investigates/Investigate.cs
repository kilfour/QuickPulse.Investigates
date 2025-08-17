namespace QuickPulse.Investigates;

public static class Investigate
{
    public static Findings These<T>(T one, T two)
    {
        return
            Signal.From(The.Flow(new FlowContext() { }))
                .SetArtery(new Findings())
                .Pulse(new Pair(one!, two!))
                .GetArtery<Findings>();
    }
}
