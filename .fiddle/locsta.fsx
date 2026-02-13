
/// Mutable value for local state within streams
type State<'s>(initial) =
    let mutable state : 's voption = initial
    member _.Value with get() = state
    member _.Set(v) = state <- ValueSome v
    override _.ToString() = state.ToString()

type SigStream<'v,'c,'s> = State<'s> -> 'c -> 'v * State<'s>

type SigStreamBuilder() =
    member inline _.Bind
        (
            [<InlineIfLambda>] m: SigStream<_,_,'sm>,
            [<InlineIfLambda>] f: _ -> SigStream<_,_,'sf>
        )
        : SigStream<_,_,_>
        =
        fun s ctx ->
            let ms,fs =
                match s.Value with
                | ValueNone -> State(ValueNone), State(ValueNone)
                | ValueSome (ms,fs) -> ms, fs
            let mv,ms = m ms ctx
            let f = f mv
            let fv,fs = f fs ctx
            do s.Set(ms, fs)
            fv, s

    member inline _.Return(x) : SigStream<_,_,unit> =
        fun s ctx -> x, s

    member inline _.ReturnFrom(v) = v

    member inline _.Zero() : SigStream<unit,_,unit> =
        fun s ctx -> (), s

    member inline _.Combine(a: SigStream<unit,_,_>, b: SigStream<_,_,_>) : SigStream<_,_,_> =
        fun s ctx ->
            let _, s = a s ctx
            b s ctx

    member inline _.Delay([<InlineIfLambda>] f) = f ()

    member inline _.For(elems: seq<_>, [<InlineIfLambda>] body) : SigStream<_,_,_> =
        fun s ctx ->
            let mutable currMap =
                match s.Value with
                | ValueNone -> Map.empty
                | ValueSome m -> m
            let resValues, resStates =
                [
                    for i, elem in elems |> Seq.indexed do
                        let elemState =
                            match currMap.TryFind(i) with
                            | Some st -> st
                            | None -> State(ValueNone)
                        let v, newState = body elem elemState ctx
                        v, (i, newState)
                ]
                |> List.unzip
            let newMap = resStates |> Map.ofList
            s.Set(newMap)
            resValues, s

let stream = SigStreamBuilder()

/// Create a stream from a sequence
let ofSeq (sequence: seq<_>) : SigStream<_,_,_> =
    fun s ctx ->
        let enumerator =
            match s.Value with
            | ValueNone -> sequence.GetEnumerator()
            | ValueSome e -> e
        match enumerator.MoveNext() with
        | true ->
            s.Set(enumerator)
            enumerator.Current, s
        | false ->
            failwith "Sequence contains no more elements"

/// Map over a stream
let inline map ([<InlineIfLambda>] proj) ([<InlineIfLambda>] s1) =
    stream {
        let! v = s1
        return proj v
    }

/// Get the context value
let inline getCtx<'c> () : SigStream<'c,'c,unit> =
    fun s ctx -> ctx, s

/// Use a memoized value (lazy initialization)
let inline useMemoWith ([<InlineIfLambda>] initializer) : SigStream<'a,'c,'a> =
    fun (state: State<'a>) ctx ->
        let value =
            match state.Value with
            | ValueNone -> initializer()
            | ValueSome v -> v
        state.Set(value)
        value, state

let useMemo (value: 'a) : SigStream<'a,'c,'a> =
    fun (state: State<'a>) ctx ->
        let v =
            match state.Value with
            | ValueNone -> value
            | ValueSome v -> v
        state.Set(v)
        v, state

/// Mutable value for local state within streams
type MutableValue<'s>(initValue: 's) =
    let mutable state = initValue
    member _.Value
        with get() = state
        and set v = state <- v
    override _.ToString() = $"mut_({state})"

/// Create stateful computation with mutable value
let inline useStateWith ([<InlineIfLambda>] initializer) =
    useMemoWith (fun () -> MutableValue(initializer()))

let useState value =
    useMemo (MutableValue(value))

module Eval =
    /// Convert stream to infinite sequence with context generator
    let inline toSeq
        ([<InlineIfLambda>] getCtx: int -> _)
        ([<InlineIfLambda>] stream: SigStream<_,_,_>)
        =
        let state = State(ValueNone)
        seq {
            let mutable i = 0
            while true do
                let ctx = getCtx i
                i <- i + 1
                let v, _ = stream state ctx
                yield v
        }

    /// Evaluate n samples
    let inline run n getCtx stream =
        toSeq getCtx stream |> Seq.take n |> Seq.toList

    /// Evaluate with input sequence
    let inline runWith (inputs: seq<_>) stream =
        let state = State(ValueNone)
        inputs
        |> Seq.map (fun ctx ->
            let v, _ = stream state ctx
            v)
        |> Seq.toList

// ============================================
// USAGE EXAMPLES
// ============================================

module Examples =

    // Example 1: Counter with state
    let counter start increment =
        stream {
            let! state = useState start
            let current = state.Value
            state.Value <- current + increment
            return current
        }

    // Example 2: Fibonacci using multiple states
    let fibonacci =
        stream {
            let! prev1 = useState 0
            let! prev2 = useState 1
            let current = prev1.Value + prev2.Value
            prev1.Value <- prev2.Value
            prev2.Value <- current
            return current
        }

    // Example 3: Moving average
    let movingAverage windowSize =
        stream {
            let! ctx = getCtx()
            let! window = useState []
            let newWindow = (ctx :: window.Value) |> List.truncate windowSize
            window.Value <- newWindow
            return (newWindow |> List.sum |> float) / (float newWindow.Length)
        }

    // Example 4: Stream combinator
    let addStreams s1 s2 =
        stream {
            let! v1 = s1
            let! v2 = s2
            return v1 + v2
        }

// Run examples:
printfn "\n╔═══════════════════════════════════════════╗"
printfn "║  LOCSTA - STATE-BASED STREAMS            ║"
printfn "╚═══════════════════════════════════════════╝\n"

printfn "=== Counter (0, increment by 1) ==="
let test1 = Examples.counter 0 1 |> Eval.toSeq id |> Seq.take 10 |> Seq.toList
printfn "%A" test1

printfn "\n=== Fibonacci ==="
let test2 = Examples.fibonacci |> Eval.toSeq (fun _ -> ()) |> Seq.take 10 |> Seq.toList
printfn "%A" test2

printfn "\n=== Moving Average (window=3, input: 1..10) ==="
let test3 = Examples.movingAverage 3 |> Eval.runWith [1.0..10.0]
printfn "%A" test3
