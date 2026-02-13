
type SigStream<'v,'c,'s> = 's voption -> 'c -> 'v * 's

type SigStreamBuilder() =
    member inline _.Bind
        (
            [<InlineIfLambda>] m: SigStream<_,_,_>,
            [<InlineIfLambda>] f: _ -> SigStream<_,_,_>
        )
        : SigStream<_,_,_>
        =
        fun s ctx ->
            let ms,fs =
                match s with
                | ValueNone -> ValueNone,ValueNone
                | ValueSome (ms,fs) -> ValueSome ms, ValueSome fs
            let mv,ms = m ms ctx
            let f = f mv
            let fv,fs = f fs ctx
            fv, (ms,fs)

    member inline _.Return(x) : SigStream<_,_,_> =
        fun s ctx -> x,()

    member inline _.ReturnFrom(v) = v

    member inline _.For(elems: seq<_>, [<InlineIfLambda>] body) : SigStream<_,_,_> =
        fun s ctx ->
            let mutable currMap = s |> ValueOption.defaultValue Map.empty
            let resValues,resStates =
                [
                    for i,elem in elems |> Seq.indexed do
                        let matchingState =
                            let found,maybeValue = currMap.TryGetValue(i)
                            if found then maybeValue else ValueNone
                        let v,s =
                            let (v: SigStream<_,_,_>) = body elem
                            let v,s = v matchingState ctx
                            v,s
                        v, (i, ValueSome s)
                ]
                |> List.unzip
            let newState = resStates |> Map.ofList
            if newState.Count <> resStates.Length then
                failwith "Duplicate key in for"
            resValues, newState

let stream = SigStreamBuilder()

/// Mutable value for local state within streams
type MutableValue<'s>(initValue: 's) =
    let mutable state = initValue
    member _.Value
        with get() = state
        and set v = state <- v
    override _.ToString() = $"mut_({state})"

/// Create a stream from a sequence
let ofSeq (sequence: seq<_>) : SigStream<_,_,_> =
    fun s ctx ->
        let enumerator = s |> ValueOption.defaultWith (fun () -> sequence.GetEnumerator())
        match enumerator.MoveNext() with
        | true -> enumerator.Current, enumerator
        | false -> failwith "Sequence contains no more elements"

/// Map over a stream
let inline map ([<InlineIfLambda>] proj) ([<InlineIfLambda>] s1) =
    stream {
        let! v = s1
        return proj v
    }

/// Get the context value
let inline getCtx<'c> () : SigStream<'c,'c,unit> =
    fun s ctx -> ctx,()

/// Use a memoized value (lazy initialization)
let inline useMemoWith ([<InlineIfLambda>] initializer) : SigStream<_,_,_> =
    fun s ctx ->
        let s = s |> ValueOption.defaultWith initializer
        s,s

let useMemo value =
    fun s ctx ->
        let s = s |> ValueOption.defaultValue value
        s,s

/// Create stateful computation with mutable value
let inline useStateWith ([<InlineIfLambda>] initializer) =
    useMemoWith (fun () -> MutableValue(initializer()))

let useState value =
    useMemo (MutableValue(value))

module Eval =
    /// Convert stream to sequence with context generator
    let inline toSeq
        ([<InlineIfLambda>] getCtx: int -> _)
        ([<InlineIfLambda>] stream: SigStream<_,_,_>)
        =
        seq {
            let mutable run = true
            let mutable currState = ValueNone
            let mutable i = 0
            while run do
                let ctx = getCtx i
                i <- i + 1
                let v,s = stream currState ctx
                do currState <- ValueSome s
                yield v
        }

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
    let test1 = counter 0 1 |> Eval.toSeq id |> Seq.take 10 |> Seq.toList
    let test2 = fibonacci |> Eval.toSeq (fun _ -> ()) |> Seq.take 10 |> Seq.toList
