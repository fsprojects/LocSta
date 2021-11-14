﻿
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

[<RequireQualifiedAccess; Struct>]
type Res<'e, 'd> =
    | Emit of emit: 'e
    | SkipWith of skip: 'd
    | Stop

type LoopEmit<'v,'s> = LoopEmit of 'v * 's
type LoopSkip<'s> = LoopSkip of 's

[<Struct>]
type Feedback<'f> =
    | UseThis of 'f
    | UseLast
    | ResetThis
    | ResetTree

type FeedEmit<'v,'s,'f> = FeedEmit of 'v * 's * 'f
type FeedSkip<'s,'f> = FeedSkip of 's * Feedback<'f>

type LoopGen<'o,'s> = Gen<Res<LoopEmit<'o,'s>, LoopSkip<'s>>, 's> 
type LoopRes<'o,'s> = Res<LoopEmit<'o,'s>, LoopSkip<'s>>

type FeedGen<'o,'s,'f> = Gen<Res<FeedEmit<'o,'s,'f>, FeedSkip<'s,'f>>, 's> 
type FeedRes<'o,'s,'f> = Res<FeedEmit<'o,'s,'f>, FeedSkip<'s,'f>>

[<Struct>]
type Init<'f> = Init of 'f

type Fx<'i,'o,'s> = 'i -> Gen<'o,'s>

[<Struct>]
type GenState<'sm, 'sk, 'm> =
    { mstate: 'sm
      kstate: 'sk option
      mleftovers: 'm list }


module Loop =
    type [<Struct>] Emit<'value> = Emit of 'value
    type [<Struct>] SkipWith<'state> = SkipWith of 'state
    type [<Struct>] Skip = Skip
    type [<Struct>] Stop = Stop


module Feed =
    type [<Struct>] Emit<'value, 'feedback> = Emit of 'value * 'feedback
    type [<Struct>] SkipWith<'state> = SkipWith of 'state
    type [<Struct>] Skip = Skip
    type [<Struct>] Stop = Stop
    // Will man Reset wirklich als Teil der Builder-Abstraktion?
    type [<Struct>] ResetThis = ResetThis
    type [<Struct>] ResetTree = ResetTree


module Res =
    let isStop result = match result with | Res.Stop -> true | _ -> false

    type AggregateResult<'e, 'd, 's> =
        { results: Res<'e, 'd> list
          resultsWithStop: Res<'e, 'd> list
          isStopped: bool
          finalState: 's option }

    let map2 valueMapping stateMapping (result: LoopRes<_,_>) =
        match result with
        | Res.Emit (LoopEmit (v,s)) -> Res.Emit (LoopEmit (valueMapping v s, stateMapping s))
        | Res.SkipWith (LoopSkip s) -> Res.SkipWith (LoopSkip (stateMapping s))
        | Res.Stop -> Res.Stop

    let map valueMapping stateMapping (result: LoopRes<_,_>) =
        map2 (fun v _ -> valueMapping v) stateMapping result

    let mapMany2 valueMapping stateMapping (results: LoopRes<_,_> list) =
        results |> List.map (map2 valueMapping stateMapping)

    let mapMany valueMapping stateMapping (results: LoopRes<_,_> list) =
        results |> List.map (map valueMapping stateMapping)

    // TODO: implement it like Loop
    let mapFeedMany valueMapping stateMapping results =
        [ for res in results do
            match res with
            | Res.Emit (FeedEmit (v,s,f)) -> Res.Emit (FeedEmit (valueMapping v, stateMapping s, f))
            | Res.SkipWith (FeedSkip (s,f)) -> Res.SkipWith (FeedSkip (stateMapping s, f))
            | Res.Stop -> Res.Stop
        ]

    let internal mapGenUntilStop tryGetValueAndState (results: Res<'e,'d> list) =
        let mappedResults =
            results
            |> Seq.map tryGetValueAndState
            |> Seq.takeWhile (fun (_,state) -> Option.isSome state)
        let resultsTilStop,finalState =
            let mutable finalState = None
            let resultsTilStop =
                seq {
                    for (res, state) in mappedResults do
                        finalState <- state
                        yield res
                }
                |> Seq.toList
            resultsTilStop,finalState
        let isStopped = results.Length > resultsTilStop.Length
        { results = resultsTilStop
          resultsWithStop =
            if isStopped
            then [ yield! resultsTilStop; yield Res.Stop ]
            else resultsTilStop
          isStopped = isStopped
          finalState = finalState }

    let internal mapFeedUntilStop valueMapping stateMapping (results: FeedRes<_,_,_> list) =
        mapGenUntilStop
            (fun res ->
                match res with
                | Res.Emit (FeedEmit (v,s,f)) -> (Res.Emit (FeedEmit (valueMapping v, stateMapping s, f))), Some s
                | Res.SkipWith (FeedSkip (s,f)) -> (Res.SkipWith (FeedSkip (stateMapping s,f))), Some s
                | Res.Stop -> Res.Stop, None)
            results

    let mapUntilStop valueMapping stateMapping (results: LoopRes<_,_> list) =
        mapGenUntilStop
            (fun res ->
                match res with
                | Res.Emit (LoopEmit (v,s)) -> (Res.Emit (LoopEmit (valueMapping v, stateMapping s))), Some s
                | Res.SkipWith (LoopSkip s) -> (Res.SkipWith (LoopSkip (stateMapping s))), Some s
                | Res.Stop -> Res.Stop, None)
            results

    let takeUntilStop results = mapUntilStop id id results
    
    let takeFeedUntilStop results = mapFeedUntilStop id id results

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

    let createGen f = Gen f
    let create f : LoopGen<_,_> = Gen f
    let createFeed f : FeedGen<_,_,_> = Gen f

    // Creates a Gen from a function that takes non-optional state, initialized with the given seed value.
    let createWithSeed f seed =
        fun s ->
            let state = Option.defaultValue seed s
            f state
        |> create

    let createWithSeed2 seed f =
        createWithSeed seed f

    
    // --------
    // bind
    // --------

    let internal bindLoopWhateverGen buildSkip processResult createWhatever k m
        =
        let evalmres mres lastKState leftovers =
            match mres with
            | Res.Emit (LoopEmit (mres, mstate)) ->
                let kgen = k mres
                let kres = run kgen lastKState
                match kres with
                | [] -> 
                    let state = { mstate = mstate; kstate = lastKState; mleftovers = leftovers }
                    [ buildSkip state ]
                | results ->
                    [ for res in results do yield processResult res mstate leftovers ]
            | Res.SkipWith (LoopSkip stateM) ->
                let state = { mstate = stateM; kstate = lastKState; mleftovers = leftovers }
                [ buildSkip state ]
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
                    [ buildSkip state ]
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
        let buildSkip state = Res.SkipWith (LoopSkip state)
        let processResult res mstate leftovers =
            match res with
            | Res.Emit (LoopEmit (kres, kstate)) ->
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.Emit (LoopEmit (kres, state))
            | Res.SkipWith (LoopSkip kstate) -> 
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.SkipWith (LoopSkip state)
            | Res.Stop ->
                Res.Stop
        bindLoopWhateverGen buildSkip processResult create k m

    let internal bindLoopFeedFeed
        (k: 'o1 -> FeedGen<'o2,'s2,'f>)
        (m: LoopGen<'o1,'s1>)
        : FeedGen<'o2,_,'f> // TODO: _
        =
        let buildSkip state = Res.SkipWith (FeedSkip (state, UseLast))
        let processResult res mstate leftovers =
            match res with
            | Res.Emit (FeedEmit (kres, kstate, kfeedback)) ->
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.Emit (FeedEmit (kres, state, kfeedback))
            | Res.SkipWith (FeedSkip (kstate, kfeedback)) -> 
                let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
                Res.SkipWith (FeedSkip (state, kfeedback))
            | Res.Stop -> 
                Res.Stop
        bindLoopWhateverGen buildSkip processResult createFeed k m

    let internal bindInitFeedLoop
        (k: 'f -> FeedGen<'o,'s,'f>)
        (m: Init<'f>)
        : LoopGen<_,_>
        =
        fun state ->
            let lastFeed, lastKState =
                let getInitial () = let (Init m) = m in m
                match state with
                | None -> getInitial (), None
                | Some { mstate = None; kstate = kstate } -> getInitial (), kstate
                | Some { mstate = Some mstate; kstate = kstate } -> mstate, kstate
            [ for res in run (k lastFeed) lastKState do
                match res with
                | Res.Emit (FeedEmit (kvalue, kstate, feedback)) ->
                    let state = { mstate = Some feedback; kstate = Some kstate; mleftovers = [] }
                    Res.Emit (LoopEmit (kvalue, state))
                | Res.SkipWith (FeedSkip (kstate, feedback)) ->
                    let feedback,kstate =
                        match feedback with
                        | UseThis feedback -> Some feedback, Some kstate
                        | UseLast -> Some lastFeed, Some kstate
                        | ResetThis -> None, Some kstate
                        | ResetTree -> None, None
                    let state = { mstate = feedback; kstate = kstate; mleftovers = [] }
                    Res.SkipWith (LoopSkip state)
                | Res.Stop ->
                    Res.Stop
            ]
        |> create


    // --------
    // returns
    // --------

    let internal ofGenResultRepeating (res: Res<_,_>) : Gen<_,_> =
        createGen (fun _ -> [ res ])

    let internal ofGenResultOnce (res: Res<_,_>) : Gen<_,_> =
        createGen (fun _ -> [ res; Res.Stop ])

    let returnSkip<'o,'s> : Gen<'o,'s> =
        createGen (fun _ -> [])
    let returnValueRepeating<'v> (value: 'v) : LoopGen<'v, unit> =
        Res.Emit (LoopEmit (value, ())) |> ofGenResultRepeating
    let returnValueOnce (value: 'v) : LoopGen<'v, unit> =
        Res.Emit (LoopEmit (value, ())) |> ofGenResultOnce
    let returnSkipWith<'v, 's> (state: 's) : LoopGen<'v,'s> =
        Res.SkipWith (LoopSkip state) |> ofGenResultRepeating
    let returnStop<'v,'s> : LoopGen<'v,'s> =
        Res.Stop |> ofGenResultRepeating
    
    let internal returnFeedback<'skip, 'v, 's, 'f> (value: 'v) (feedback: 'f) : FeedGen<'v, unit, 'f> =
        Res.Emit (FeedEmit (value, (), feedback)) |> ofGenResultRepeating
    let internal returnFeedbackSkip<'v, 'f> : FeedGen<'v, unit, 'f>  =
        Res.SkipWith (FeedSkip ((), UseLast)) |> ofGenResultRepeating
    let internal returnFeedbackSkipWith<'v, 'f> (feedback: 'f) : FeedGen<'v, unit, 'f>  =
        Res.SkipWith (FeedSkip ((), UseThis feedback)) |> ofGenResultRepeating
    let internal returnFeedbackResetThis<'v, 'f> : FeedGen<'v, unit, 'f>  =
        Res.SkipWith (FeedSkip ((), ResetThis)) |> ofGenResultRepeating
    let internal returnFeedbackResetTree<'v, 'f> : FeedGen<'v, unit, 'f>  =
        Res.SkipWith (FeedSkip ((), ResetTree)) |> ofGenResultRepeating
    let internal returnFeedbackStop<'v,'s,'f> : FeedGen<'v,'s,'f> =
        Res.Stop |> ofGenResultRepeating


    // --------
    // seq / list
    // --------

    let ofSeq (s: seq<_>) =
        fun enumerator ->
            let enumerator = enumerator |> Option.defaultWith (fun () -> s.GetEnumerator())
            let nextValue =
                match enumerator.MoveNext() with
                | true -> Res.Emit (LoopEmit (enumerator.Current, enumerator))
                | false -> Res.Stop
            [ nextValue ]
        |> create
        
    let ofList (list: list<_>) =
        fun l ->
            let l = l |> Option.defaultValue list
            let res =
                match l with
                | x::xs -> Res.Emit (LoopEmit (x, xs))
                | [] -> Res.Stop
            [ res ]
        |> create

    type OnStopThenState<'s> =
        | RunInput of 's option
        | Default

    let inline internal onStopThenResult defaultResults (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            let state = state |> Option.defaultValue (RunInput None)
            match state with
            | Default ->
                defaultResults
            | RunInput state ->
                let res =
                    run inputGen state
                    |> Res.mapUntilStop id (fun state -> RunInput (Some state)) 
                if res.isStopped then
                    [
                        yield! res.results
                        yield! defaultResults
                    ]
                else
                    res.results
        |> createGen
        
    let inline onStopThenDefault defaultValue (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenResult [ Res.Emit (LoopEmit (defaultValue, Default)) ] inputGen

    let inline onStopThenSkip (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenResult [] inputGen


    // --------
    // combine
    // --------

    type CombineInfo<'sa, 'sb> =
        { astate: 'sa option
          bstate: 'sb option }

    let internal combineLoop
        (a: LoopGen<'o, 'sa>)
        (b: unit -> LoopGen<'o, 'sb>)
        : LoopGen<'o, CombineInfo<'sa,'sb>>
        =
        fun state ->
            let state =  state |> Option.defaultValue { astate = None; bstate = None }
            let aresults = run a state.astate |> Res.takeUntilStop
            let mappedAResults =
                aresults.resultsWithStop
                |> Res.mapMany id (fun sa -> { astate = Some sa; bstate = None })
            let mappedBResults =            
                match aresults.isStopped with
                | false ->
                    run (b()) state.bstate |> Res.takeUntilStop
                    |> fun res -> res.resultsWithStop
                    |> Res.mapMany id (fun sb -> { astate = aresults.finalState; bstate = Some sb })
                | true ->
                    []
            mappedAResults @ mappedBResults
        |> create

    // TODO: Redundant with combine
    let internal combineFeed
        (a: FeedGen<'o, 'sa, 'f>)
        (b: unit -> FeedGen<'o, 'sb, 'f>)
        : FeedGen<'o, CombineInfo<'sa,'sb>, 'f>
        =
        fun state ->
            let state =  state |> Option.defaultValue { astate = None; bstate = None }
            let aresults = run a state.astate |> Res.takeFeedUntilStop
            let mappedAResults =
                aresults.resultsWithStop
                |> Res.mapFeedMany id (fun sa -> { astate = Some sa; bstate = None })
            let mappedBResults =            
                match aresults.isStopped with
                | false ->
                    run (b()) state.bstate |> Res.takeFeedUntilStop
                    |> fun res -> res.resultsWithStop
                    |> Res.mapFeedMany id (fun sb -> { astate = aresults.finalState; bstate = Some sb })
                | true ->
                    []
            mappedAResults @ mappedBResults
        |> createFeed


    // --------
    // Builder
    // --------

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = ofList x
        member _.Zero() = returnSkip
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type LoopBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        member _.Combine(x, delayed) = combineLoop x delayed
        member _.For(sequence: seq<'a>, body) = ofSeq sequence |> bind body
        // returns
        member _.Return(Loop.Emit value) = returnValueRepeating value
        member _.Yield(value: 'a) = returnValueRepeating value
        member _.Return(Loop.SkipWith state) = returnSkipWith state
        member _.Return(Loop.Skip) = returnSkip
        member _.Return(Loop.Stop) = returnStop
        
    type FeedBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bindInitFeedLoop f m
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindLoopFeedFeed f m
        member _.Combine(x, delayed) = combineLoop x delayed
        member _.Combine(x, delayed) = combineFeed x delayed
        member _.For(sequence: seq<'a>, body) =
            ofSeq sequence |> bindLoopFeedFeed body
        // returns
        member _.Return(Feed.Emit (value, feedback)) = returnFeedback value feedback
        member _.Yield(value: 'v, feedback: 'f) = returnFeedback value feedback
        member _.Return(Feed.SkipWith state) = returnFeedbackSkipWith state
        member _.Return(Feed.Skip) = returnSkip
        member _.Return(Feed.Stop) = returnFeedbackStop
        member _.Return(Feed.ResetThis) = returnFeedbackResetThis
        member _.Return(Feed.ResetTree) = returnFeedbackResetTree
    
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
                        | Res.SkipWith (LoopSkip fstate) ->
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
                            | Res.SkipWith (LoopSkip stateF) ->
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
            yield f l r
        }
    
    let inline binOpLeft left right f =
        Gen.loop {
            let l = left
            let! r = right
            yield f l r
        }
    
    let inline binOpRight left right f =
        Gen.loop {
            let! l = left
            let r = right
            yield f l r
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