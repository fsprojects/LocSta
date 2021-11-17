
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

// TODO: why is 's optional - what does it mean exactly? Could mean: Reset or UseLast.
//       Is it important to specify that or will it defined by the library?
[<RequireQualifiedAccess>]
type Res<'v,'s> =
    | Continue of 'v list * 's
    | Stop of 'v list

type LoopState<'s> = LoopState of 's option
type LoopRes<'o,'s> = Res<'o, LoopState<'s>>
type LoopGen<'o,'s> = Gen<LoopRes<'o,'s>, 's> 

[<Struct>]
type Feedback<'f> =
    | UseThis of 'f
    | UseLast
    | ResetThis
    | ResetTree

type FeedState<'s,'f> = FeedState of 's option * Feedback<'f> option
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
    type [<Struct>] Emit<'value> = Emit of 'value  // TODO: 'value list
    type [<Struct>] Skip = Skip
    type [<Struct>] Stop<'value> = Stop of 'value  // TODO: 'value list


module Feed =
    type [<Struct>] Emit<'value, 'feedback> = Emit of 'value * 'feedback  // TODO: 'value list
    type [<Struct>] SkipWith<'feedback> = SkipWith of 'feedback
    type [<Struct>] Stop<'value> = Stop of 'value  // TODO: 'value list
    // Will man Reset wirklich als Teil der Builder-Abstraktion?
    type [<Struct>] ResetThis = ResetThis                                 // TODO: 'value list oder 'value
    type [<Struct>] ResetTree = ResetTree                                 // TODO: 'value list oder 'value


module Gen =
    
    let run (gen: Gen<_,_>) = let (Gen b) = gen in b


    // --------
    // Gen creation
    // --------

    let createGen f = Gen f
    let create f : LoopGen<_,_> = Gen f
    let createFeed f : FeedGen<_,_,_> = Gen f

    
    // --------
    // bind
    // --------

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
                | Res.Continue (kvalues, LoopState kstate) ->
                    let state = { mstate = mstate; kstate = kstate; mleftovers = mleftovers; isStopped = isStopped }
                    Res.Continue (kvalues, LoopState (Some state))
                | Res.Stop kvalues ->
                    Res.Stop kvalues
            let evalmres mres lastMState lastKState isStopped =
                match mres with
                | Res.Continue (mval :: mleftovers, LoopState mstate) ->
                    evalk mval mstate mleftovers lastKState isStopped
                | Res.Continue ([], LoopState mstate) ->
                    let state = { mstate = mstate; kstate = lastKState; mleftovers = []; isStopped = isStopped }
                    Res.Continue ([], LoopState (Some state))
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
        fun state ->
            let evalk mval mstate mleftovers lastKState isStopped =
                match run (k mval) lastKState with
                | Res.Continue (kvalues, FeedState (kstate, feedback)) ->
                    let state = { mstate = mstate; kstate = kstate; mleftovers = mleftovers; isStopped = isStopped }
                    Res.Continue (kvalues, FeedState (Some state, feedback))
                | Res.Stop kvalues ->
                    Res.Stop kvalues
            let evalmres mres lastMState lastKState isStopped =
                match mres with
                | Res.Continue (mval :: mleftovers, LoopState mstate) ->
                    evalk mval mstate mleftovers lastKState isStopped
                | Res.Continue ([], LoopState mstate) ->
                    let state = { mstate = mstate; kstate = lastKState; mleftovers = []; isStopped = isStopped }
                    Res.Continue ([], FeedState (Some state, None))
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
        |> createFeed
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
                | Res.Continue (kvalues, FeedState (kstate, Some feedback)) ->
                    let feedback,kstate =
                        match feedback with
                        | UseThis feedback -> Some feedback, kstate
                        | UseLast -> Some lastFeed, kstate
                        | ResetThis -> None, kstate
                        | ResetTree -> None, None
                    let state = { mstate = feedback; kstate = kstate; mleftovers = []; isStopped = false }
                    Res.Continue (kvalues, LoopState (Some state))
                | Res.Continue (kvalues, FeedState (kstate, None)) ->
                    let state = { mstate = Some lastFeed; kstate = kstate; mleftovers = []; isStopped = false }
                    Res.Continue (kvalues, LoopState (Some state))
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

    let internal returnLoopRes res =
        (fun _ -> res) |> create
    let internal returnFeedRes res =
        (fun _ -> res) |> createFeed

    // TODO: Schauen, ob dieses Vokabular noch Sinn ergibt

    let returnContinue<'v, 's> values state : LoopGen<'v,'s> =
        returnLoopRes (Res.Continue (values, state))
    let returnStop<'v,'s> values : LoopGen<'v,'s> =
        returnLoopRes (Res.Stop values)
    
    let returnContinueFeed<'v,'s,'f> values state : FeedGen<'v,'s,'f> =
        returnFeedRes (Res.Continue (values, state))
    let returnStopFeed<'v,'s,'f> values : FeedGen<'v,'s,'f> =
        returnFeedRes (Res.Stop values)


    // --------
    // seq / list
    // --------

    // TODO: think about dropping ofSeq support completely
    let ofSeq (s: seq<_>) =
        fun enumerator ->
            let enumerator = enumerator |> Option.defaultWith (fun () -> s.GetEnumerator())
            match enumerator.MoveNext() with
            | true -> Res.Continue ([enumerator.Current], LoopState (Some enumerator))
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
            | x::xs -> Res.Continue ([x], LoopState (Some xs))
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
                Res.Continue (defaultValues, LoopState (Some UseDefault))
            | RunInput state ->
                match run inputGen state with
                | Res.Continue (values, LoopState state) ->
                    Res.Continue (values, LoopState (Some (RunInput state)))
                | Res.Stop values ->
                    Res.Continue (values, LoopState (Some UseDefault))
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
            | Res.Continue (avalues, LoopState astate) ->
                match run (b()) state.bstate with
                | Res.Continue (bvalues, LoopState bstate) ->
                    Res.Continue (avalues @ bvalues, LoopState (Some { astate = astate; bstate = bstate }))
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
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()

    type LoopBuilder() =
        inherit BaseBuilder()
        member _.Zero() = returnContinue [] (LoopState None)
        member _.Bind(m, f) = bind f m
        //member _.For(sequence: list<'a>, body) = ofList sequence |> onStopThenSkip |> bind body
        //member _.For(sequence: seq<'a>, body) = ofSeq sequence |> onStopThenSkip |> bind body
        member _.Combine(x, delayed) = combineLoop x delayed
        // returns
        member _.Return(Loop.Emit value) = returnContinue [value] (LoopState None)
        member _.Yield(value) : LoopGen<_,_> = returnContinue [value] (LoopState None)
        member _.Return(Loop.Skip) = returnContinue [] (LoopState None)
        member _.Return(Loop.Stop value) = returnStop [value]
        
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
        member _.Return(Feed.Emit (value, feedback)) =
            returnContinueFeed [value] (FeedState(None, Some(UseThis feedback)))
        member _.Yield(value, feedback) =
            returnContinueFeed [value] (FeedState(None, Some(UseThis feedback)))
        member _.Return(Feed.SkipWith feedback) = 
            returnContinueFeed [] (FeedState(None, Some(UseThis feedback)))
        member _.Return(Feed.Stop value) = 
            returnStopFeed [value]
        //member _.Return(Feed.ResetThis) = returnFeedbackResetThis // TODO (siehe Kommentar oben)
        //member _.Return(Feed.ResetTree) = returnFeedbackResetTree // TODO (siehe Kommentar oben)
    
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

    let toSeq (g: LoopGen<_,'s>) : seq<_> =
        let f = run g
        let mutable state = None
        let mutable resume = true
        seq {
            while resume do
                match f state with
                | Res.Continue (values, LoopState (Some fstate)) ->
                    state <- Some fstate
                    yield! values
                | Res.Continue (values, LoopState None) ->
                    yield! values
                | Res.Stop values ->
                    resume <- false
                    yield! values
        }
    
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
