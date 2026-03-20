[<AutoOpen>]
module LocSta.Core

/// Mutable value for local state within streams
type StateController<'s>(initial) =
    let mutable state : 's voption = initial
    member _.Value with get() = state
    member _.Set(v) = state <- ValueSome v
    member _.Reset() = state <- ValueNone
    override _.ToString() = state.ToString()

type SigStream<'v,'c,'s> = StateController<'s> -> 'c -> 'v seq * StateController<'s>

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
                | ValueNone -> StateController(ValueNone), StateController(ValueNone)
                | ValueSome (ms,fs) -> ms, fs
            let mvs,ms = m ms ctx
            // Fast path for single-element arrays (most common case)
            match mvs with
            | :? ('a array) as arr when arr.Length = 1 ->
                let fStream = f arr[0]
                let fvs, newFs = fStream fs ctx
                do s.Set(ms, newFs)
                fvs, s
            | _ ->
                let results = ResizeArray()
                let mutable currentFs = fs
                for mv in mvs do
                    let fStream = f mv
                    let fvs, newFs = fStream currentFs ctx
                    for fv in fvs do results.Add(fv)
                    currentFs <- newFs
                do s.Set(ms, currentFs)
                results :> seq<_>, s

    member inline _.Return(x) : SigStream<_,_,unit> =
        fun s _ -> [| x |] :> seq<_>, s

    member inline _.Yield(x) : SigStream<_,_,unit> =
        fun s _ -> [| x |] :> seq<_>, s

    member inline _.ReturnFrom(v) = v

    member inline _.YieldFrom(v) = v

    member inline _.Zero() : SigStream<_,_,unit> =
        fun s _ -> Seq.empty, s

    member inline _.Combine(a: SigStream<_,_,_>, b: SigStream<_,_,_>) : SigStream<_,_,_> =
        fun s ctx ->
            let avs, s = a s ctx
            let bvs, s = b s ctx
            Seq.append avs bvs, s

    member inline _.Delay([<InlineIfLambda>] f) = f ()


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
            [| enumerator.Current |] :> seq<_>, s
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
    fun s ctx -> [| ctx |] :> seq<_>, s

/// Get the state controller of this block
let inline getState<'c,'s> () : SigStream<StateController<'s>,'c,'s> =
    fun state ctx ->
        [| state |] :> seq<_>, state

/// Use a memoized value (lazy initialization)
let inline useMemoWith ([<InlineIfLambda>] initializer) : SigStream<'a,'c,'a> =
    fun (state: StateController<'a>) ctx ->
        let value =
            match state.Value with
            | ValueNone -> initializer()
            | ValueSome v -> v
        state.Set(value)
        [| value |] :> seq<_>, state

let useMemo (value: 'a) : SigStream<'a,'c,'a> =
    fun (state: StateController<'a>) ctx ->
        let v =
            match state.Value with
            | ValueNone -> value
            | ValueSome v -> v
        state.Set(v)
        [| v |] :> seq<_>, state

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
    /// Convert stream to sequence with context generator (flattens multi-value emissions)
    let inline toSeq
        ([<InlineIfLambda>] getCtx: int -> _)
        ([<InlineIfLambda>] stream: SigStream<_,_,_>)
        =
        let state = StateController(ValueNone)
        seq {
            let mutable i = 0
            while true do
                let ctx = getCtx i
                i <- i + 1
                let vs, _ = stream state ctx
                yield! vs
        }

    /// Evaluate n samples
    let inline run (n: int) getCtx stream =
        let state = StateController(ValueNone)
        let results = ResizeArray<_>(n)
        let mutable i = 0
        while results.Count < n do
            let ctx = getCtx i
            i <- i + 1
            let vs, _ = stream state ctx
            for v in vs do
                if results.Count < n then results.Add(v)
        Seq.toList results

    /// Evaluate with input sequence
    let inline runWith (inputs: seq<_>) stream =
        let state = StateController(ValueNone)
        let results = ResizeArray<_>()
        for ctx in inputs do
            let vs, _ = stream state ctx
            for v in vs do results.Add(v)
        Seq.toList results
