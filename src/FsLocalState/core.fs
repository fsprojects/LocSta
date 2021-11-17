
(*
A hint for the type argument names:
    * 'o: output of a Gen.
    * 'v: value; the actual value emitted by a Gen - usually corresponds to the output of a Gen.
    * 'f: feedback
    * 's: state
For the API surface, names like 'value or 'state are used instead of chars.
*)

namespace FsLocalState

type Gen<'o,'s> = Gen of ('s option -> 'o)

[<RequireQualifiedAccess>]
type Res<'v,'s> =
    | Continue of 'v list * 's option // TODO: why is 's optional? Do we need "DefaultableLoopState"?
    | Stop of 'v list

type LoopState<'s> = LoopState of 's
type LoopRes<'o,'s> = Res<'o, LoopState<'s>>
type LoopGen<'o,'s> = Gen<LoopRes<'o,'s>, 's> 

[<Struct>]
type Feedback<'f> =
    | UseThis of 'f
    | UseLast
    | ResetThis
    | ResetTree

type FeedState<'s,'f> = FeedState of 's * Feedback<'f>
type FeedRes<'o,'s,'f> = Res<'o, FeedState<'s,'f>>
type FeedGen<'o,'s,'f> = Gen<FeedRes<'o,'s,'f>, 's> 

[<Struct>]
type Init<'f> = Init of 'f

type Fx<'i,'o,'s> = 'i -> Gen<'o,'s>

[<Struct>]
type BindState<'sm, 'sk, 'm> =
    { mstate: 'sm option       // this is optional because m can have no state by just yielding Stop
      kstate: 'sk option       // means: k evaluates to a constant; "return" uses this.
      mleftovers: 'm list
      isStopped: bool }


module Loop =
    type [<Struct>] Emit<'value> = Emit of 'value // TODO: 'value list
    type [<Struct>] Skip = Skip
    type [<Struct>] Stop<'value> = Stop of 'value  // TODO: 'value list


module Feed =
    type [<Struct>] Emit<'value, 'feedback> = Emit of 'value * 'feedback  // TODO: 'value list
    type [<Struct>] SkipWith<'feedback> = SkipWith of 'feedback
    type [<Struct>] Stop<'value> = Stop of 'value  // TODO: 'value list
    // Will man Reset wirklich als Teil der Builder-Abstraktion?
    type [<Struct>] ResetThis = ResetThis
    type [<Struct>] ResetTree = ResetTree


//module Res =
//    let isStop result = match result with | Res.Stop -> true | _ -> false

//    type AggregateResult<'e,'d,'s> =
//        { results: Res<'e, 'd> list
//          resultsWithStop: Res<'e, 'd> list
//          isStopped: bool
//          finalState: 's option }

//    let map2 valueMapping stateMapping (result: LoopRes<_,_>) =
//        match result with
//        | Res.Emit (LoopState (v,s)) -> Res.Emit (LoopState (valueMapping v s, stateMapping s))
//        | Res.SkipWith (LoopSkip s) -> Res.SkipWith (LoopSkip (stateMapping s))
//        | Res.Stop -> Res.Stop

//    let map valueMapping stateMapping (result: LoopRes<_,_>) =
//        map2 (fun v _ -> valueMapping v) stateMapping result

//    let mapMany2 valueMapping stateMapping (results: LoopRes<_,_> list) =
//        results |> List.map (map2 valueMapping stateMapping)

//    let mapMany valueMapping stateMapping (results: LoopRes<_,_> list) =
//        results |> List.map (map valueMapping stateMapping)

//    // TODO: implement it like Loop
//    let mapFeedMany valueMapping stateMapping results =
//        [ for res in results do
//            match res with
//            | Res.Emit (FeedEmit (v,s,f)) -> Res.Emit (FeedEmit (valueMapping v, stateMapping s, f))
//            | Res.SkipWith (FeedState (s,f)) -> Res.SkipWith (FeedState (stateMapping s, f))
//            | Res.Stop -> Res.Stop
//        ]

//    let internal mapGenUntilStop tryGetValueAndState (results: Res<'e,'d> list) =
//        let mappedResults =
//            results
//            |> Seq.map tryGetValueAndState
//            |> Seq.takeWhile (fun (_,state) -> Option.isSome state)
//        let resultsTilStop,finalState =
//            let mutable finalState = None
//            let resultsTilStop =
//                seq {
//                    for (res, state) in mappedResults do
//                        finalState <- state
//                        yield res
//                }
//                |> Seq.toList
//            resultsTilStop,finalState
//        let isStopped = results.Length > resultsTilStop.Length
//        { results = resultsTilStop
//          resultsWithStop =
//            if isStopped
//            then [ yield! resultsTilStop; yield Res.Stop ]
//            else resultsTilStop
//          isStopped = isStopped
//          finalState = finalState }

//    let internal mapFeedUntilStop valueMapping stateMapping (results: FeedRes<_,_,_> list) =
//        mapGenUntilStop
//            (fun res ->
//                match res with
//                | Res.Emit (FeedEmit (v,s,f)) -> (Res.Emit (FeedEmit (valueMapping v, stateMapping s, f))), Some s
//                | Res.SkipWith (FeedState (s,f)) -> (Res.SkipWith (FeedState (stateMapping s,f))), Some s
//                | Res.Stop -> Res.Stop, None)
//            results

//    let mapUntilStop valueMapping stateMapping (results: LoopRes<_,_> list) =
//        mapGenUntilStop
//            (fun res ->
//                match res with
//                | Res.Emit (LoopState (v,s)) -> (Res.Emit (LoopState (valueMapping v, stateMapping s))), Some s
//                | Res.SkipWith (LoopSkip s) -> (Res.SkipWith (LoopSkip (stateMapping s))), Some s
//                | Res.Stop -> Res.Stop, None)
//            results

//    let takeUntilStop results = mapUntilStop id id results
    
//    let takeFeedUntilStop results = mapFeedUntilStop id id results

//    let emittedValues (results: LoopRes<_,_> list) =
//        results
//        |> List.choose (fun res ->
//            match res with
//            | Res.Emit (LoopState (v, _)) -> Some v
//            | _ -> None
//        )


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

    let (|OptionalLoopState|) =
        function 
        | Some (LoopState state) -> Some state
        | None -> None

    // TODO: remove redundancies below like it was before
    //let internal bindLoopWhateverGen processResult createWhatever k m
    //    =
    //    fun state ->
    //        let lastMState, lastKState, lastLeftovers =
    //            match state with
    //            | None -> None, None, []
    //            | Some state -> Some state.mstate, state.kstate, state.mleftovers
    //        let evalk mres leftovers =
    //            match mres with
    //            | Res.Continue (mvalues, mstate) ->
    //                let kgen = k mres
    //                let kres = run kgen lastKState
    //                match kres with
    //                | [] -> 
    //                    let state = { mstate = mstate; kstate = lastKState; mleftovers = leftovers }
    //                    [ buildSkip state ]
    //                | results ->
    //                    [ for res in results do yield processResult res mstate leftovers ]
    //            | Res.Stop mvalues ->
    //                [ Res.Stop ]
    //        match lastLeftovers with
    //        | x :: xs -> evalk x xs
    //        | [] ->
    //            match run m lastMState with
    //            | res :: leftovers ->
    //                evalk res leftovers
    //            | [] ->
    //                match lastMState with
    //                | Some lastStateM ->
    //                    let state = { mstate = lastStateM; kstate = lastKState; mleftovers = [] }
    //                    [ buildSkip state ]
    //                | None ->
    //                    []
    //    |> createWhatever

    let bind
        (k: 'o1 -> LoopGen<'o2,'s2>)
        (m: LoopGen<'o1,'s1>)
        : LoopGen<'o2, BindState<'s1,'s2,'o1>>
        =           
        fun state ->
            let evalk mval mstate mleftovers lastKState isStopped =
                match run (k mval) lastKState with
                | Res.Continue (kvalues, OptionalLoopState kstate) ->
                    let state = { mstate = mstate; kstate = kstate; mleftovers = mleftovers; isStopped = isStopped }
                    Res.Continue (kvalues, Some (LoopState state))
                | Res.Stop kvalues ->
                    Res.Stop kvalues
            let evalmres mres lastMState lastKState isStopped =
                match mres with
                | Res.Continue (mval :: mleftovers, OptionalLoopState mstate) ->
                    evalk mval mstate mleftovers lastKState isStopped
                | Res.Continue ([], OptionalLoopState mstate) ->
                    let state = { mstate = mstate; kstate = lastKState; mleftovers = []; isStopped = isStopped }
                    Res.Continue ([], Some (LoopState state))
                | Res.Stop (mval :: mleftovers) ->
                    evalk mval lastMState mleftovers lastKState isStopped
                | Res.Stop [] ->
                    Res.Stop []
            match state with
            | Some { mstate = lastMState; mleftovers = x :: xs; kstate = lastKState; isStopped = isStopped } ->
                evalk x lastMState xs lastKState isStopped
            | Some { mleftovers = []; isStopped = true } ->
                Res.Stop []
            | Some { mstate = lastMState; mleftovers = []; kstate = kstate } ->
                evalmres (run m lastMState) lastMState kstate false
            | None ->
                evalmres (run m None) None None false
        |> create

    let internal bindLoopFeedFeed
        (k: 'o1 -> FeedGen<'o2,'s2,'f>)
        (m: LoopGen<'o1,'s1>)
        //: FeedGen<'o2,_,'f> // TODO: _
        =
            failwith "TODO"
        //let buildSkip state = Res.SkipWith (FeedState (state, UseLast))
        //let processResult res mstate leftovers =
        //    match res with
        //    | Res.Emit (FeedEmit (kres, kstate, kfeedback)) ->
        //        let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
        //        Res.Emit (FeedEmit (kres, state, kfeedback))
        //    | Res.SkipWith (FeedState (kstate, kfeedback)) -> 
        //        let state = { mstate = mstate; kstate = Some kstate; mleftovers = leftovers }
        //        Res.SkipWith (FeedState (state, kfeedback))
        //    | Res.Stop -> 
        //        Res.Stop
        //bindLoopWhateverGen buildSkip processResult createFeed k m

    let internal bindInitFeedLoop
        (k: 'f -> FeedGen<'o,'s,'f>)
        (m: Init<'f>)
        : LoopGen<_,_>
        =
        fun state ->
            let getInitial () = let (Init m) = m in m
            let evalk lastFeed lastKState =
                match run (k lastFeed) lastKState with
                | Res.Continue (kvalues, Some (FeedState (kstate, feedback))) ->
                    let feedback,kstate =
                        match feedback with
                        | UseThis feedback -> Some feedback, Some kstate
                        | UseLast -> Some lastFeed, Some kstate
                        | ResetThis -> None, Some kstate
                        | ResetTree -> None, None
                    let state = { mstate = feedback; kstate = kstate; mleftovers = []; isStopped = false }
                    Res.Continue (kvalues, Some (LoopState state))
                | Res.Continue (kvalues, None) ->
                    let state = { mstate = Some lastFeed; kstate = None; mleftovers = []; isStopped = false }
                    Res.Continue (kvalues, Some (LoopState state))
                | Res.Stop kvalues ->
                    Res.Stop kvalues
            match state with
            | Some { isStopped = true } ->
                Res.Stop []
            | None ->
                evalk (getInitial()) None
            | Some { mstate = None; kstate = kstate } ->
                evalk (getInitial()) kstate
            | Some { mstate = Some feedback; kstate = kstate } ->
                evalk feedback kstate
        |> create


    // --------
    // returns
    // --------

    let returnGenResult (res: Res<'v,'s>) : Gen<Res<'v,'s>,'s> =
        (fun _ -> res) |> createGen

    // Loop
    let returnSkip<'o,'s> : Gen<Res<'o,'s>,'s> =
        returnGenResult (Res.Continue ([], None))
    let returnValueRepeating<'v, 's> (value: 'v) : Gen<Res<'v,'s>, 's> =
        returnGenResult (Res.Continue ([value], None))
    let returnValueOnce (value: 'v) =
        returnGenResult (Res.Stop [value])
    let returnSkipWith<'v, 's> (state: 's) : Gen<Res<'v,'s>,'s> =
        returnGenResult (Res.Continue ([], Some state))
    let returnStop<'v,'s> (value: 'v) : Gen<Res<'v,'s>,'s> =
        returnGenResult (Res.Stop [value])


    // --------
    // seq / list
    // --------

    // TODO: think about dropping ofSeq support completely
    let ofSeq (s: seq<_>) =
        fun enumerator ->
            let enumerator = enumerator |> Option.defaultWith (fun () -> s.GetEnumerator())
            match enumerator.MoveNext() with
            | true -> Res.Continue ([enumerator.Current], Some (LoopState enumerator))
            | false -> Res.Stop []
        |> create
        
    // TODO: Könnten eigentlich 2 Funktionen sein:
    //          a) Liste komplett abspulen, dann weiter
    //          b) pairwise
    //               ^------------- erstmal das hier
    let ofList (list: list<_>) =
        fun l ->
            let l = l |> Option.defaultValue list
            match l with
            | x::xs -> Res.Continue ([x], Some (LoopState xs))
            | [] -> Res.Stop []
        |> create

    type OnStopThenState<'s> =
        | RunInput of 's option
        | UseDefault

    let inline internal onStopThenValues defaultValues (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            let state = state |> Option.defaultValue (RunInput None)
            match state with
            | UseDefault ->
                Res.Continue (defaultValues, Some (LoopState (UseDefault)))
            | RunInput state ->
                match run inputGen state with
                | Res.Continue (values, OptionalLoopState state) ->
                    Res.Continue (values, Some (LoopState (RunInput state)))
                | Res.Stop values ->
                    Res.Continue (values, Some (LoopState (UseDefault)))
        |> createGen
        
    let inline onStopThenDefault defaultValue (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenValues [defaultValue] inputGen

    let inline onStopThenSkip (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenValues [] inputGen


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
            match run a state.astate with
            | Res.Continue (avalues, OptionalLoopState astate) ->
                match run (b()) state.bstate with
                | Res.Continue (bvalues, OptionalLoopState bstate) ->
                    Res.Continue (avalues @ bvalues, Some (LoopState ({ astate = astate; bstate = bstate })))
                | Res.Stop bvalues ->
                    Res.Stop (avalues @ bvalues)
            | Res.Stop avalues ->
                Res.Stop avalues
        |> create

    // TODO: Redundant with combine
    //let internal combineFeed
    //    (a: FeedGen<'o, 'sa, 'f>)
    //    (b: unit -> FeedGen<'o, 'sb, 'f>)
    //    : FeedGen<'o, CombineInfo<'sa,'sb>, 'f>
    //    =
    //    fun state ->
    //        let state =  state |> Option.defaultValue { astate = None; bstate = None }
    //        match run a state.astate with
    //        | Res.Continue (avalues, astate) ->
    //            match run (b()) state.bstate with
    //            | Res.Continue (bvalues, bstate) ->
    //                Res.Continue (avalues @ bvalues, Some (LoopState ({ astate = astate; bstate = bstate })))
    //            | Res.Stop bvalues ->
    //                Res.Stop (avalues @ bvalues)
    //        | Res.Stop avalues ->
    //            Res.Stop avalues
    //        //let mappedAResults =
    //        //    aresults.resultsWithStop
    //        //    |> Res.mapFeedMany id (fun sa -> { astate = Some sa; bstate = None })
    //        //let mappedBResults =            
    //        //    match aresults.isStopped with
    //        //    | false ->
    //        //        run (b()) state.bstate |> Res.takeFeedUntilStop
    //        //        |> fun res -> res.resultsWithStop
    //        //        |> Res.mapFeedMany id (fun sb -> { astate = aresults.finalState; bstate = Some sb })
    //        //    | true ->
    //        //        []
    //        //mappedAResults @ mappedBResults
    //    |> createFeed


    // --------
    // For
    // --------

    //let forList (l: 'a list) (body: 'a -> Gen<'o, 's>) =
    //    fun state ->
    //        let state = state |> Option.defaultValue (List.init l.Length (fun _ -> None))
    //        let tmp = List.zip l state
    //        let results = [ for v,s in List.zip l state do body v |> run s ]
    //    |> create


    // --------
    // Builder
    // --------

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = ofList x // TODO: test this
        member _.Zero() = returnSkip
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type LoopBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bind f m
        //member _.For(sequence: list<'a>, body) = ofList sequence |> onStopThenSkip |> bind body
        //member _.For(sequence: seq<'a>, body) = ofSeq sequence |> onStopThenSkip |> bind body
        member _.Combine(x, delayed) = combineLoop x delayed
        // returns
        member _.Return(Loop.Emit value) = returnValueRepeating value
        member _.Yield(value: 'a) = returnValueRepeating value
        member _.Return(Loop.Skip) = returnSkip
        member _.Return(Loop.Stop value) = returnStop value
        
    type FeedBuilder() =
        inherit BaseBuilder()
        member _.Bind(m, f) = bindInitFeedLoop f m
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindLoopFeedFeed f m
        member _.For(sequence: list<'a>, body) = ofList sequence |> onStopThenSkip |> bindLoopFeedFeed body
        //member _.For(sequence: seq<'a>, body) = ofSeq sequence |> onStopThenSkip |> bindLoopFeedFeed body
        member _.Combine(x, delayed) = combineLoop x delayed
        //member _.Combine(x, delayed) = combineFeed x delayed // TODO
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
                        | Res.Emit (LoopState (fres, fstate)) ->
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
                            | Res.Emit (LoopState (resF, stateF)) ->
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
