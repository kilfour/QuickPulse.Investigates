using QuickPulse.Bolts;
using QuickPulse.Show;

namespace QuickPulse.Investigates;

public static class The
{
    private static string Show(object obj)
        => Introduce.This(obj, false);

    private readonly static Flow<Pair> Default =
        from input in Pulse.Start<Pair>()
        from _ in Pulse.TraceIf<FlowContext>(
            _ => !Equals(input.This, input.That),
            a => $"{a.GetTracePrefix()}{Show(input.This)} /= {Show(input.That)}")
        select input;

    private readonly static Flow<(int, Pair)> Element =
        from input in Pulse.Start<(int Index, Pair Pair)>()
        from _ in Pulse.Scoped<FlowContext>(a => a.AddIndex(input.Index),
            Pulse.ToFlow(Dispatch!, input.Pair))
        select input;

    private readonly static Flow<Pair> Collection =
        from input in Pulse.Start<Pair>()
        from _ in Pulse.ToFlow(Element, Get.IndexedPairs(input))
        select input;

    private readonly static Flow<ObjectProperty> Property =
        from input in Pulse.Start<ObjectProperty>()
        from _ in Pulse.Scoped<FlowContext>(a => a.AddTrace(input.Name),
            Pulse.ToFlow(Dispatch!, input.pair))
        select input;

    private readonly static Flow<ObjectProperty> KeyValue =
        from input in Pulse.Start<ObjectProperty>()
        from _ in Pulse.Scoped<FlowContext>(a => a.AddKey(input.Name),
            Pulse.ToFlow(Dispatch!, input.pair))
        select input;

    private readonly static Flow<Pair> Dictionary =
        from input in Pulse.Start<Pair>()
        from _ in Pulse.ToFlow(KeyValue, Get.DictionaryEntries(input))
        select input;

    private readonly static Flow<Pair> Tuple =
        from input in Pulse.Start<Pair>()
        from _ in Pulse.ToFlow(KeyValue, Get.TupleObjectProperties(input))
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
            (() => input.This == null || input.That == null,      /**/ () => Pulse.ToFlow(Default, input)),
            (() => flowContext.Value.AlreadyVisited(input),       /**/ () => Pulse.NoOp()),
            (() => Is.Dictionary(input.This),                     /**/ () => Pulse.ToFlow(Dictionary, input)),
            (() => Is.Collection(input.This),                     /**/ () => Pulse.ToFlow(Collection, input)),
            (() => Is.Tuple(input.This),                          /**/ () => Pulse.ToFlow(Tuple, input)),
            (() => Is.Object(input.This),                         /**/ () => Pulse.ToFlow(Object, input)),
            (() => true,                                          /**/ () => Pulse.ToFlow(Default, input)))
        select input;

    public static Flow<Pair> Flow(FlowContext FlowContext) =>
        from input in Pulse.Start<Pair>()
        from _ in Pulse.Gather(FlowContext)
        from __ in Pulse.ToFlow(Dispatch, input)
        select input;
}
