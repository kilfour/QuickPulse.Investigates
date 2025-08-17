using QuickPulse.Bolts;
using QuickPulse.Show;

namespace QuickPulse.Investigates;

public static class TheFlow
{
    private readonly static Flow<Pair> Fallback =
        from input in Pulse.Start<Pair>()
        from indent in Pulse.TraceIf<FlowContext>(
            _ => !Equals(input.This, input.That),
            a => $"{a.GetTracePrefix()}{Introduce.This(input.This, false)} /= {Introduce.This(input.That, false)}")
        select input;

    private readonly static Flow<ObjectProperty> Property =
    from input in Pulse.Start<ObjectProperty>()
    from _ in Pulse.Scoped<FlowContext>(a => a.AddTrace(input.Name),
        Pulse.ToFlow(Dispatch!, input.pair))
    select input;

    private readonly static Flow<Pair> Object =
        from input in Pulse.Start<Pair>()
        let items = Get.ObjectProperties(input)
        from _ in Pulse.ToFlow(Property, Get.ObjectProperties(input))
        select input;

    private readonly static Flow<Pair> Dispatch =
        from input in Pulse.Start<Pair>()
        from flowContext in Pulse.Gather(new FlowContext())
        from _ in Pulse.FirstOf(
            (() => Is.Object(input.This),  /**/ () => Pulse.ToFlow(Object, input)),
            (() => true,                   /**/ () => Pulse.ToFlow(Fallback, input)))
        select input;

    public static Flow<Pair> Go(FlowContext FlowContext) =>
        from input in Pulse.Start<Pair>()
        from _ in Pulse.Gather(FlowContext)
        from __ in Pulse.ToFlow(Dispatch, input)
        select input;
}
