module LocSta

/// Mutable value for local state within streams
type MutableValue<'s>(initValue: 's) =
    let mutable state = initValue
    member _.Value
        with get() = state
        and set v = state <- v
    override _.ToString() = $"mut_({state})"

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

    /// Evaluate n samples
    let inline run n getCtx stream =
        toSeq getCtx stream |> Seq.take n |> Seq.toList

    /// Evaluate with input sequence
    let inline runWith (inputs: seq<_>) stream =
        let mutable currState = ValueNone
        inputs
        |> Seq.map (fun ctx ->
            let v,s = stream currState ctx
            currState <- ValueSome s
            v)
        |> Seq.toList
