
(*
A hint for the type argument names:
    * 'o: output of a Gen.
    * 'v: value; the actual value emitted by a Gen - usually corresponds to the output of a Gen.
    * 'f: feedback
    * 's: state
For the API surface, names like 'value or 'state are used instead of chars.
*)

namespace FsLocalState

type Gen<'o,'s> = Gen of ('s option -> 'o list)

// TODO: rename DiscardWith to Discard
// TODO: 'f and f (as function name in bind) can be distracting
[<RequireQualifiedAccess>]
type Res<'e, 'd> =
    | Emit of 'e
    | DiscardWith of 'd
    | Stop

type LoopEmit<'v,'s> = LoopEmit of 'v * 's
type LoopDiscard<'s> = LoopDiscard of 's

type FeedEmit<'v,'s,'f> = FeedEmit of 'v * 's * 'f
type FeedDiscard<'s,'f> = FeedDiscard of 's * 'f option

type LoopGen<'o,'s> = Gen<Res<LoopEmit<'o,'s>, LoopDiscard<'s>>, 's> 
type LoopRes<'o,'s> = Res<LoopEmit<'o,'s>, LoopDiscard<'s>>

type FeedGen<'o,'s,'f> = Gen<Res<FeedEmit<'o,'s,'f>, FeedDiscard<'s,'f>>, 's> 
type FeedRes<'o,'s,'f> = Res<FeedEmit<'o,'s,'f>, FeedDiscard<'s,'f>>

type Init<'f> = Init of 'f

type Fx<'i,'o,'s> = 'i -> Gen<'o,'s>

[<Struct>]
type GenState<'sm, 'sk, 'm> =
    { mstate: 'sm
      kstate: 'sk option
      mleftovers: 'm list }


module Loop =
    type Emit<'value> = Emit of 'value
    type DiscardWith<'state> = DiscardWith of 'state
    type Stop = Stop


module Feed =
    type Feedback<'value, 'feedback> = Feedback of 'value * 'feedback
    type DiscardWith<'state> = DiscardWith of 'state
    type Stop = Stop


module Res =
    let isStop result = match result with | Res.Stop -> true | _ -> false

    type AggregateResult<'o, 's> =
        { results: LoopRes<'o, 's> list
          isStopped: bool
          finalState: 's option }

    let mapUntilStop mapping (results: LoopRes<_,_> list) =
        // TODO: Implement a "UntilStopResult" that doesn't have 'Stop' as case and get rid of the failwith.
        let resultsTilStop, finalState =
            results
            |> Seq.takeWhile (isStop >> not)
            |> Seq.mapFold
                (fun _ res ->
                    let newState = 
                        match res with
                        | Res.Emit (LoopEmit (_, s)) -> Some s
                        | Res.DiscardWith (LoopDiscard s) -> Some s
                        | Res.Stop -> failwith "Stop is not supported."
                    mapping res, newState
                )
                None
            |> fun (results, state) -> results |> Seq.toList, state
        { results = resultsTilStop
          isStopped = results.Length > resultsTilStop.Length
          finalState = finalState }

    let takeUntilStop results = mapUntilStop id results

    let emittedValues (results: LoopRes<_,_> list) =
        results
        |> List.choose (fun res ->
            match res with
            | Res.Emit (LoopEmit (v, _)) -> Some v
            | _ -> None
        )


module Gen =
    
    let run (gen: Gen<_,_>) = let (Gen b) = gen in b


    // --------
    // Gen creation
    // --------

    /// Single case DU constructor.
    let create f = Gen f
    let createGen f : LoopGen<_,_> = Gen f
    let createFdb f : FeedGen<_,_,_> = Gen f

    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let createWithSeed f seed =
        fun s ->
            let state = Option.defaultValue seed s
            f state
        |> createGen

    let createWithSeed2 seed f =
        createWithSeed seed f

    
    // --------
    // bind
    // --------

    let internal bindLoopWhateverGen discard processResult createWhatever k m
        =
        let evalmres mres lastKState leftovers =
            match mres with
            | Res.Emit (LoopEmit (mres, mstate)) ->
                let kgen = k mres
                let kres = run kgen lastKState
                match kres with
                | [] -> 
                    let state = { mstate = mstate; kstate = lastKState; mleftovers = leftovers }
                    [ discard state ]
                | results ->
                    [ for res in results do yield processResult res mstate leftovers ]
            | Res.DiscardWith (LoopDiscard stateM) ->
                let state = { mstate = stateM; kstate = lastKState; mleftovers = leftovers }
                [ discard state ]
            | Res.Stop ->
                [ Res.Stop ]
        let rec evalm lastMState lastKState =
            match run m lastMState with
            | res :: leftovers ->
                evalmres res lastKState leftovers
            | [] ->
                match lastMState with
                | Some lastStateM ->
                    let state = { mstate = lastStateM; kstate = lastKState; mleftovers = [] }
                    [ discard state ]
                | None ->
                    []
        fun state ->
            let lastMState, lastKState, lastLeftovers =
                match state with
                | None -> None, None, []
                | Some state -> Some state.mstate, state.kstate, state.mleftovers
            match lastLeftovers with
            | x :: xs -> evalmres x lastKState xs
            | [] -> evalm lastMState lastKState
        |> createWhatever

    let bind
        (k: 'o1 -> LoopGen<'o2, 's2>)
        (m: LoopGen<'o1, 's1>)
        : LoopGen<'o2, GenState<'s1, 's2, LoopRes<'o1, 's1>>>
        =
        let discard state = Res.DiscardWith (LoopDiscard state)
        let processResult res mstate leftovers =
            match res with
            | Res.Emit (LoopEmit (kres, kstate)) ->
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.Emit (LoopEmit (kres, state))
            | Res.DiscardWith (LoopDiscard kstate) -> 
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.DiscardWith (LoopDiscard state)
            | Res.Stop ->
                Res.Stop
        bindLoopWhateverGen discard processResult createGen k m

    let internal bindLoopFeedFeed
        (k: 'o1 -> FeedGen<'o2,'s2,'f>)
        (m: LoopGen<'o1,'s1>)
        : FeedGen<'o2,_,'f> // TODO: _
        =
        let discard state = Res.DiscardWith (FeedDiscard (state, None))
        let processResult res mstate leftovers =
            match res with
            | Res.Emit (FeedEmit (kres, kstate, kfeedback)) ->
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.Emit (FeedEmit (kres, state, kfeedback))
            | Res.DiscardWith (FeedDiscard (kstate, kfeedback)) -> 
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.DiscardWith (FeedDiscard (state, kfeedback))
            | Res.Stop -> 
                Res.Stop
        bindLoopWhateverGen discard processResult createFdb k m

    let internal bindInitFdbGen
        (k: 'f -> FeedGen<'o,'s,'f>)
        (m: Init<'f>)
        : LoopGen<_,_>
        =
        fun state ->
            let lastFeed, lastKState =
                match state with
                | None -> let (Init m) = m in m, None
                | Some state  -> state.mstate, state.kstate
            [ for res in run (k lastFeed) lastKState do
                match res with
                | Res.Emit (FeedEmit (kvalue, kstate, feedback)) ->
                    let state = { mstate = feedback; kstate = Some kstate; mleftovers = [] }
                    Res.Emit (LoopEmit (kvalue, state))
                | Res.DiscardWith (FeedDiscard (kstate, feedback)) ->
                    let feedback =
                        match feedback with
                        | Some feedback -> feedback
                        | None -> lastFeed
                    let state = { mstate = feedback; kstate = Some kstate; mleftovers = [] }
                    Res.DiscardWith (LoopDiscard state)
                | Res.Stop ->
                    Res.Stop
            ]
        |> createGen


    // --------
    // return / yield
    // --------

    let internal ofGenResultRepeating (res: Res<_,_>) : Gen<_,_> =
        create (fun _ -> [ res ])

    let internal ofGenResultOnce (res: Res<_,_>) : Gen<_,_> =
        create (fun _ -> [ res; Res.Stop ])

    let returnValueRepeating<'v> (value: 'v) : LoopGen<'v, unit> =
        Res.Emit (LoopEmit (value, ())) |> ofGenResultRepeating
    
    let returnValueOnce (value: 'v) : LoopGen<'v, unit> =
        Res.Emit (LoopEmit (value, ())) |> ofGenResultOnce
    
    let returnDiscardWith<'v, 's> (state: 's) : LoopGen<'v,'s> =
        Res.DiscardWith (LoopDiscard state) |> ofGenResultRepeating
    
    let returnStop<'v,'s> : LoopGen<'v,'s> =
        Res.Stop |> ofGenResultRepeating
    
    let returnFeedbackStop<'v,'s,'f> : FeedGen<'v,'s,'f> =
        Res.Stop |> ofGenResultRepeating
    
    let returnFeedback<'discard, 'v, 's, 'f> (value: 'v) (feedback: 'f) : FeedGen<'v, unit, 'f> =
        Res.Emit (FeedEmit (value, (), feedback)) |> ofGenResultRepeating
    
    let returnFeedbackDiscardWith<'v, 'f> (feedback: 'f) : FeedGen<'v, unit, 'f>  =
        Res.DiscardWith (FeedDiscard ((), Some feedback)) |> ofGenResultRepeating


    // --------
    // singleton / seq / list
    // --------

    let ofSeq (s: seq<_>) =
        fun enumerator ->
            let enumerator = enumerator |> Option.defaultWith (fun () -> s.GetEnumerator())
            let nextValue =
                match enumerator.MoveNext() with
                | true -> Res.Emit (LoopEmit (enumerator.Current, enumerator))
                | false -> Res.Stop
            [ nextValue ]
        |> createGen
        
    let ofList (list: list<_>) =
        fun l ->
            let l = l |> Option.defaultValue list
            let res =
                match l with
                | x::xs -> Res.Emit (LoopEmit (x, xs))
                | [] -> Res.Stop
            [ res ]
        |> createGen


    // --------
    // combine
    // --------

    type CombineInfo<'sa, 'sb> =
        { astate: 'sa option
          bstate: 'sb option }

    let combine 
        (a: LoopGen<'o, 'sa>)
        (b: unit -> LoopGen<'o, 'sb>)
        =
        let getValue g state = run g state
        fun state ->
            [
                let state =  state |> Option.defaultValue { astate = None; bstate = None }
                
                let mutable astate = state.astate
                let mutable isRunning = true

                // TODO: that looks quite crappy, buy maybe it's ok?
                // TODO: redundancy
                for res in getValue a state.astate do
                    if isRunning then
                        match res with
                        | Res.Emit (LoopEmit (va, sa)) ->
                            astate <- Some sa
                            yield Res.Emit (LoopEmit (va, { astate = astate; bstate = None }))
                        | Res.DiscardWith (LoopDiscard sa) -> 
                            astate <- Some sa
                            yield Res.DiscardWith (LoopDiscard { astate = astate; bstate = None })
                        | Res.Stop ->
                            isRunning <- false
                            yield Res.Stop
                if isRunning then
                    for res in getValue (b ()) state.bstate do
                        if isRunning then
                            match res with
                            | Res.Emit (LoopEmit (vb, sb)) ->
                                yield Res.Emit (LoopEmit (vb, { astate = astate; bstate = Some sb }))
                            | Res.DiscardWith (LoopDiscard sb) -> 
                                yield Res.DiscardWith (LoopDiscard { astate = astate; bstate = Some sb })
                            | Res.Stop ->
                                isRunning <- false
                                yield Res.Stop
            ]
        |> createGen


    // --------
    // Builder
    // --------

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = ofList x
        member _.Zero() = Gen (fun _ -> [])
        member _.For(sequence: seq<'a>, body) = ofSeq sequence |> bind body
        member _.Combine(x, delayed) = combine x delayed
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type LoopBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        // returns
        member _.Return(Loop.Emit value) = returnValueRepeating value
        member _.Return(Loop.DiscardWith state) = returnDiscardWith state
        member _.Return(Loop.Stop) = returnStop
        
    type FeedBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bindInitFdbGen f m
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindLoopFeedFeed f m
        // returns
        member _.Return(Feed.Feedback (value, feedback)) = returnFeedback value feedback
        member _.Return(Feed.DiscardWith state) = returnFeedbackDiscardWith state
        member _.Return(Feed.Stop) = returnFeedbackStop
    
    let loop = LoopBuilder()
    let feed = FeedBuilder()


    // -------
    // Kleisli
    // -------

    let pipe (g: Fx<_,_,_>) (f: Gen<_,_>) : Gen<_,_> =
        loop {
            let! f' = f
            return! g f' 
        }

    let pipeFx (g: Fx<_,_,_>) (f: Fx<_,_,_>): Fx<_,_,_> =
        fun x -> loop {
            let! f' = f x
            return! g f' 
        }

    
    // -------
    // Evaluation
    // -------
    
    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let resumeOrStart (state: 's option) (g: LoopGen<_,'s>) =
        let f = run g
        let mutable state = state
        let mutable resume = true
        seq {
            while resume do
                for res in f state do
                    if resume then
                        match res with
                        | Res.Emit (LoopEmit (fres, fstate)) ->
                            state <- Some fstate
                            yield (fres, fstate)
                        | Res.DiscardWith (LoopDiscard fstate) ->
                            state <- Some fstate
                        | Res.Stop ->
                            resume <- false
        }
    
    let resume state (g: LoopGen<_,'s>) = resumeOrStart (Some state) g

    // TODO: Document this
    /// Be careful: This uses a state machine, which means:
    /// A mutable object is used as state!
    let toSeqStateFx (fx: Fx<'i,_,'s>) : seq<'i> -> seq<_ * 's> =
        let mutable state = None
        let mutable resume = true

        fun inputValues ->
            seq {
                let enumerator = inputValues.GetEnumerator()
                while enumerator.MoveNext() && resume do
                    let value = enumerator.Current
                    let fxres = run (fx value) state
                    for res in fxres do
                        if resume then
                            match res with
                            | Res.Emit (LoopEmit (resF, stateF)) ->
                                state <- Some stateF
                                yield (resF, stateF)
                            | Res.DiscardWith (LoopDiscard stateF) ->
                                state <- Some stateF
                            | Res.Stop ->
                                resume <- false
            }

    let toSeqFx (fx: Fx<'i,_,'s>) : seq<'i> -> seq<_> =
        let evaluable = toSeqStateFx fx
        fun inputValues -> evaluable inputValues |> Seq.map fst
    
    let toSeqState (g: LoopGen<_,_>) = resumeOrStart None g
    
    let toSeq (g: LoopGen<_,_>) = toSeqState g |> Seq.map fst

    let toListFx fx inputSeq =
        inputSeq |> toSeqFx fx |> Seq.toList
    
    let toList gen =
        toSeq gen |> Seq.toList
    
    let toListn count gen =
        toSeq gen |> Seq.truncate count |> Seq.toList


[<RequireQualifiedAccess>]
module Arithmetic =
    let inline binOpBoth left right f =
        Gen.loop {
            let! l = left
            let! r = right
            return Loop.Emit (f l r)
        }
    
    let inline binOpLeft left right f =
        Gen.loop {
            let l = left
            let! r = right
            return Loop.Emit (f l r)
        }
    
    let inline binOpRight left right f =
        Gen.loop {
            let! l = left
            let r = right
            return Loop.Emit (f l r)
        }


type Gen<'o,'s> with
    // the 'comparison' constraint is a hack to prevent ambiguities in
    // F# operator overload resolution.

    // TODO: document operators and especially ==
    
    static member inline (+) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (+)
    static member inline (-) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (-)
    static member inline (*) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (*)
    static member inline (/) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (/)
    static member inline (%) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (%)
    static member inline (==) (left: ^a when ^a: comparison, right) = Arithmetic.binOpLeft left right (=)

    static member inline (+) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (+)
    static member inline (-) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (-)
    static member inline (*) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (*)
    static member inline (/) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (/)
    static member inline (%) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (%)
    static member inline (==) (left, right: ^a when ^a: comparison) = Arithmetic.binOpRight left right (=)

    static member inline (+) (left, right) = Arithmetic.binOpBoth left right (+)
    static member inline (-) (left, right) = Arithmetic.binOpBoth left right (-)
    static member inline (*) (left, right) = Arithmetic.binOpBoth left right (*)
    static member inline (/) (left, right) = Arithmetic.binOpBoth left right (/)
    static member inline (%) (left, right) = Arithmetic.binOpBoth left right (%)
    static member inline (==) (left, right) = Arithmetic.binOpBoth left right (=)


open System.Runtime.CompilerServices

[<Extension>]
type Extensions() =
    [<Extension>]
    static member GetSlice(inputGen: LoopGen<'o, 's>, inclStartIdx, inclEndIdx) =
        let s = max 0 (defaultArg inclStartIdx 0)
        let e = max 0 (defaultArg inclEndIdx 0)
        let l = min 0 (e - s)
        inputGen |> Gen.toSeq |> Seq.skip s |> Seq.truncate l |> Gen.ofSeq


[<AutoOpen>]
module TopLevelOperators =

    /// Kleisli operator (fx >> fx)
    let (>=>) f g = Gen.pipeFx g f

    /// Kleisli "pipe" operator (gen >> fx)
    let (|=>) f g = Gen.pipe g f

    /// Bind operator
    let (>>=) m f = Gen.bind f m

    let loop = Gen.loop
    let feed = Gen.feed
