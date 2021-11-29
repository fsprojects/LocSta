﻿
(*
A hint for the type argument names:
    * 'o: output of a Gen.
    * 'v: value; the actual value emitted by a Gen - usually corresponds to the output of a Gen.
    * 'f: feedback
    * 's: state
For the API surface, names like 'value or 'state are used instead of chars.
*)

// TODO: InlineIfLambda, ValueOptions, Structs
namespace FsLocalState

type Gen<'o,'s> = Gen of ('s option -> 'o)

// TODO: Is it really a good idea generalizing Res instead of using disjoint results for Feed and Loop?
[<RequireQualifiedAccess>]
type Res<'v,'s> =
    | Continue of 'v list * 's
    | Stop of 'v list

[<RequireQualifiedAccess>]
type LoopState<'s> =
    | Update of 's
    | KeepLast
    | Reset
type LoopRes<'o,'s> = Res<'o, LoopState<'s>>
type LoopGen<'o,'s> = Gen<LoopRes<'o,'s>, 's>

[<RequireQualifiedAccess>]
type FeedType<'f> =
    | Update of 'f
    | KeepLast
    | Reset
    | ResetFeedback
    | ResetDescendants of 'f

type FeedState<'s,'f> = FeedState of 's option * FeedType<'f>
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

/// Convenience for working directly with Gen funcs.
module Res =
    module Loop =
        let emitMany values state = Res.Continue (values, LoopState.Update state)
        let emitManyAndKeepLast values = Res.Continue (values, LoopState.KeepLast)
        let emitManyAndReset values = Res.Continue (values, LoopState.Reset)
        let emitManyAndStop values = Res.Stop values
        let emit value state = emitMany [value] state
        let emitAndKeepLast value = emitManyAndKeepLast [value]
        let emitAndReset value = emitManyAndReset [value]
        let emitAndStop value = emitManyAndStop [value]
        let skip state = emitMany [] state
        let skipAndKeepLast<'v,'s> = emitManyAndKeepLast [] : Res<'v, LoopState<'s>>
        let skipAndReset<'v,'s> = emitManyAndReset [] : Res<'v, LoopState<'s>>
        let stop<'v,'s> = emitManyAndStop [] : Res<'v, LoopState<'s>>

/// Vocabulary for Return of loop CE.
module Loop =
    type [<Struct>] Emit<'value> = Emit of 'value
    type [<Struct>] EmitAndReset<'value> = EmitAndReset of 'value
    type [<Struct>] EmitAndStop<'value> = EmitAndStop of 'value

    type [<Struct>] EmitMany<'value> = EmitMany of 'value list
    type [<Struct>] EmitManyAndReset<'value> = EmitManyAndReset of 'value list
    type [<Struct>] EmitManyAndStop<'value> = EmitManyAndStop of 'value list

    type [<Struct>] Skip = Skip
    type [<Struct>] SkipAndReset = SkipAndReset
    type [<Struct>] Stop = Stop

/// Vocabulary for Return of feed CE.
module Feed =
    type [<Struct>] Emit<'value, 'feedback> = Emit of 'value * 'feedback
    type [<Struct>] EmitAndReset<'value> = EmitAndReset of 'value
    type [<Struct>] EmitAndResetFeedback<'value> = EmitAndResetFeedback of 'value
    type [<Struct>] EmitAndResetDescendants<'value, 'feedback> = EmitAndResetDescendants of 'value * 'feedback
    type [<Struct>] EmitAndStop<'value> = EmitAndStop of 'value

    type [<Struct>] EmitMany<'value, 'feedback> = EmitMany of 'value list * 'feedback
    type [<Struct>] EmitManyAndReset<'value> = EmitManyAndReset of 'value list
    type [<Struct>] EmitManyAndResetFeedback<'value> = EmitManyAndResetFeedback of 'value list
    type [<Struct>] EmitManyAndResetDescendants<'value, 'feedback> = EmitManyAndResetDescendants of 'value list * 'feedback
    type [<Struct>] EmitManyAndStop<'value> = EmitManyAndStop of 'value list

    type [<Struct>] Skip<'feedback> = Skip of 'feedback
    type [<Struct>] SkipAndReset = SkipAndReset
    type [<Struct>] SkipAndResetFeedback = SkipAndResetFeedback
    type [<Struct>] SkipAndResetDescendants<'feedback> = SkipAndResetDescendants of 'feedback
    type [<Struct>] Stop = Stop


// TODO: make some things internal and expose them explicitly via Gen type
[<AutoOpen>]
module Gen =

    let run (gen: Gen<_,_>) = let (Gen b) = gen in b


    // --------
    // Gen creation
    // --------

    let createGen f = Gen f
    let createLoop f : LoopGen<_,_> = Gen f
    let createFeed f : FeedGen<_,_,_> = Gen f

    
    // --------
    // Active Recognizers
    // --------

    let (|LoopStateToOption|) defaultState currState =
        match currState with
        | LoopState.Update s -> Some s
        | LoopState.KeepLast -> defaultState
        | LoopState.Reset -> None
    
    // --------
    // bind
    // --------

    // TODO: Bräuchte buildState nicht auch den lastState? Und as TODO von unten - welche Rolle spielt das?
    let internal bindLoopWhateverGen evalk buildSkip createWhatever m =
        fun state ->
            let evalmres mres lastMState lastKState isStopped =
                match mres with
                | Res.Continue (mval :: mleftovers, LoopStateToOption lastMState mstate) ->
                    evalk mval mstate mleftovers lastKState isStopped
                | Res.Continue ([], LoopStateToOption lastMState mstate) ->
                    let state = { mstate = mstate; kstate = lastKState; mleftovers = []; isStopped = isStopped }
                    // TODO: why "None" in case of Res.Continue?
                    Res.Continue ([], buildSkip state)
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
        |> createWhatever

    let bind
        (k: 'o1 -> LoopGen<'o2,'s2>)
        (m: LoopGen<'o1,'s1>)
        : LoopGen<'o2, BindState<'s1,'s2,'o1>>
        =
        let evalk mval mstate mleftovers lastKState isStopped =
            match run (k mval) lastKState with
            | Res.Continue (kvalues, kstate) ->
                let newState kstate = { mstate = mstate; kstate = kstate; mleftovers = mleftovers; isStopped = isStopped }
                match kstate with
                | LoopState.Update kstate ->
                    Res.Continue (kvalues, LoopState.Update (newState (Some kstate)))
                | LoopState.KeepLast ->
                    Res.Continue (kvalues, LoopState.Update (newState lastKState))
                | LoopState.Reset ->
                    Res.Continue (kvalues, LoopState.Reset)
            | Res.Stop kvalues ->
                Res.Stop kvalues
        let buildSkip state = LoopState.Update state
        bindLoopWhateverGen evalk buildSkip createLoop m

    let internal bindLoopFeedFeed
        (k: 'o1 -> FeedGen<'o2,'s2,'f>)
        (m: LoopGen<'o1,'s1>)
        : FeedGen<'o2,_,'f> // TODO: _
        =
        let evalk mval mstate mleftovers lastKState isStopped =
            match run (k mval) lastKState with
            | Res.Continue (kvalues, FeedState (kstate, feedState)) ->
                let state = { mstate = mstate; kstate = kstate; mleftovers = mleftovers; isStopped = isStopped }
                Res.Continue (kvalues, FeedState (Some state, feedState))
            | Res.Stop kvalues ->
                Res.Stop kvalues
        let buildSkip state = FeedState (Some state, FeedType.KeepLast)
        bindLoopWhateverGen evalk buildSkip createFeed m

    let internal bindInitFeedLoop
        (k: 'f -> FeedGen<'o,'s,'f>)
        (m: Init<'f>)
        : LoopGen<_,_>
        =
        fun state ->
            let getInitial () = let (Init m) = m in m
            let evalk lastFeed lastKState =
                match run (k lastFeed) lastKState with
                | Res.Continue (kvalues, FeedState (kstate, feedback)) ->
                    let feedback,kstate =
                        match feedback with                                                                                                                                                                                                                                             
                        | FeedType.Update feedback -> Some feedback, kstate
                        | FeedType.KeepLast -> Some lastFeed, kstate
                        | FeedType.Reset -> None, None
                        | FeedType.ResetFeedback -> None, kstate
                        | FeedType.ResetDescendants feedback -> Some feedback, None
                    let state = { mstate = feedback; kstate = kstate; mleftovers = []; isStopped = false }
                    Res.Continue (kvalues, LoopState.Update state)
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
        |> createLoop


    // --------
    // create of values
    // --------

    let inline internal returnLoop res = createLoop (fun _ -> res)
    let inline internal returnFeedRes res = createFeed (fun _ -> res)

    let ofRepeatingValues<'v, 's> values : LoopGen<'v,'s> = returnLoop (Res.Continue (values, LoopState.KeepLast))
    let ofRepeatingValue<'v, 's> value : LoopGen<'v,'s> = ofRepeatingValues [value]
    let ofOneTimeValues<'v, 's> values : LoopGen<'v,'s> = returnLoop (Res.Stop values)
    let ofOneTimeValue<'v, 's> value : LoopGen<'v,'s> = ofOneTimeValues [value]


    // --------
    // create of seq / list
    // --------

    // TODO: think about dropping ofSeq support completely
    let ofSeqOneByOne (s: seq<_>) =
        fun enumerator ->
            let enumerator = enumerator |> Option.defaultWith (fun () -> s.GetEnumerator())
            match enumerator.MoveNext() with
            | true -> Res.Loop.emit enumerator.Current enumerator
            | false -> Res.Loop.stop
        |> createLoop

    // TODO: Improve naming

    /// Emits the head of the list and retains the excess or stops on an empty list.
    let ofListOneByOne (list: list<_>) =
        fun l ->
            let l = l |> Option.defaultValue list
            match l with
            | x::xs -> Res.Loop.emit x xs
            | [] -> Res.Loop.stop
        |> createLoop

    /// Emits the complete list or stopps on an empty list.
    let ofListAllAtOnce (list: list<_>) =
        fun _ ->
            match list with
            | [] -> Res.Loop.stop
            | l -> Res.Loop.emitManyAndKeepLast l
        |> createLoop


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
            let ares = run a state.astate
            match ares with
            | Res.Continue (avalues, LoopStateToOption state.astate astate) ->
                let bres = run (b()) state.bstate
                match bres with
                | Res.Continue (bvalues, LoopStateToOption state.bstate bstate) ->
                    Res.Continue (avalues @ bvalues, LoopState.Update { astate = astate; bstate = bstate })
                | Res.Stop bvalues ->
                    Res.Stop (avalues @ bvalues)
            | Res.Stop avalues ->
                Res.Stop avalues
        |> createLoop

    // TODO: Redundant with combine
    // TODO: verstehe ich noch nicht ganz: was passiert denn mit afeedback? -> Testen:
    // Wie genau verhält es sich, wenn ich 2 feeds combine (und 'fa, 'fb, 'fc)?
    let internal combineFeed
        (a: FeedGen<'o, 'sa, 'f>)
        (b: unit -> FeedGen<'o, 'sb, 'f>)
        : FeedGen<'o, CombineInfo<'sa,'sb>, 'f>
        =
        fun state ->
            let state =  state |> Option.defaultValue { astate = None; bstate = None }
            match run a state.astate with
            // TODO: document this: 'afeedback' is unused, which means: the last emitted feedback is used when combining
            | Res.Continue (avalues, FeedState (astate, _ (* nowarn for afeedback *) )) ->
                match run (b()) state.bstate with
                | Res.Continue (bvalues, FeedState (bstate, bfeedback)) ->
                    let state = { astate = astate; bstate = bstate }
                    Res.Continue (avalues @ bvalues, FeedState (Some state, bfeedback))
                | Res.Stop bvalues ->
                    Res.Stop (avalues @ bvalues)
            | Res.Stop avalues ->
                Res.Stop avalues
        |> createFeed


    // -------
    // evaluation / transform to other domains
    // -------
    
    // TODO: same pattern (resumeOrStart, etc.) as in Gen also for Fx

    let toSeq (g: LoopGen<_,'s>) : seq<_> =
        let f = run g
        let mutable state = None
        let mutable resume = true
        seq {
            while resume do
                match f state with
                | Res.Continue (values, LoopStateToOption state fstate) ->
                    state <- fstate
                    yield! values
                | Res.Stop values ->
                    resume <- false
                    yield! values
        }

    // TODO: quite redundant with toSeq, but wrapping it's 'g' seems inefficient
    let toSeqFx (fx: 'i -> LoopGen<'o,'s>) : seq<'i> -> seq<'o> =
        let mutable state = None
        let mutable resume = true
        fun inputValues ->
            let enumerator = inputValues.GetEnumerator()
            seq {
                while resume && enumerator.MoveNext() do
                    match run (fx enumerator.Current) state with
                    | Res.Continue (values, LoopStateToOption state fstate) ->
                        state <- fstate
                        yield! values
                    | Res.Stop values ->
                        resume <- false
                        yield! values
            }
    
    let toList gen =
        toSeq gen |> Seq.toList

    let toListn count gen =
        toSeq gen |> Seq.truncate count |> Seq.toList

    let toListFx fx input =
        input |> toSeqFx fx |> Seq.toList

    let toFx (gen: Gen<'s, 'o>) : Fx<unit, 's, 'o> =
        fun () -> gen


    // --------
    // Builder
    // --------

    type BaseBuilder() =
        member _.ReturnFrom(x) = x
        member _.YieldFrom(x) = ofListAllAtOnce x
        member _.Delay(delayed) = delayed
        member _.Run(delayed) = delayed ()
        member _.For(list: list<_>, body) = list |> toListFx body |> ofListAllAtOnce
        // TODO: member _.For(sequence: seq<'a>, body) = ofSeq sequence |> onStopThenSkip |> bind body

    type LoopBuilder() =
        inherit BaseBuilder()
        member _.Zero() = returnLoop Res.Loop.skipAndKeepLast
        member _.Bind(m, f) = bind f m
        member _.Combine(x, delayed) = combineLoop x delayed
        
        member _.Yield(value) : LoopGen<_,_> = returnLoop (Res.Loop.emitAndKeepLast value)

        // TODO: Die müssen alle in coreLoopTests abgetestet sein
        member _.Return(Loop.Emit value) = returnLoop (Res.Loop.emitAndKeepLast value)
        member _.Return(Loop.EmitAndReset value) = returnLoop (Res.Loop.emitAndReset value)
        member _.Return(Loop.EmitAndStop value) = returnLoop (Res.Loop.emitAndStop value)

        member _.Return(Loop.EmitMany values) = returnLoop (Res.Loop.emitManyAndKeepLast values)
        member _.Return(Loop.EmitManyAndReset values) = returnLoop (Res.Loop.emitManyAndReset values)
        member _.Return(Loop.EmitManyAndStop values) = returnLoop (Res.Loop.emitManyAndStop values)

        member _.Return(Loop.Skip) = returnLoop Res.Loop.skipAndKeepLast
        member _.Return(Loop.SkipAndReset) = returnLoop Res.Loop.skipAndReset
        member _.Return(Loop.Stop) = returnLoop Res.Loop.stop
        
    type FeedBuilder() =
        inherit BaseBuilder()
        
        let cont values feedType =
            returnFeedRes (Res.Continue (values, FeedState (None, feedType)))
        
        member _.Zero() = cont [] FeedType.KeepLast
        member _.Bind(m, f) = bindInitFeedLoop f m
        member _.Bind(m, f) = bind f m
        member _.Bind(m, f) = bindLoopFeedFeed f m
        member _.Combine(x, delayed) = combineLoop x delayed
        member _.Combine(x, delayed) = combineFeed x delayed
        
        // TODO: Die müssen alle in coreLoopTests abgetestet sein

        member _.Yield(value, feedback) = cont [value] (FeedType.Update feedback)

        member _.Return(Feed.Emit (value, feedback)) = cont [value] (FeedType.Update feedback)
        member _.Return(Feed.EmitAndReset value) = cont [value] FeedType.Reset
        member _.Return(Feed.EmitAndResetFeedback value) = cont [value] FeedType.ResetFeedback
        member _.Return(Feed.EmitAndResetDescendants (value, feedback)) = cont [value] (FeedType.ResetDescendants feedback)
        member _.Return(Feed.EmitAndStop value) = returnFeedRes (Res.Stop [value])

        member _.Return(Feed.EmitMany (values, feedback)) = cont values (FeedType.Update feedback)
        member _.Return(Feed.EmitManyAndReset values) = cont values FeedType.Reset
        member _.Return(Feed.EmitManyAndResetFeedback values) = cont values FeedType.ResetFeedback
        member _.Return(Feed.EmitManyAndResetDescendants (values, feedback)) = cont values (FeedType.ResetDescendants feedback)
        member _.Return(Feed.EmitManyAndStop values) = returnFeedRes (Res.Stop values)

        member _.Return(Feed.Skip feedback) = cont [] (FeedType.Update feedback)
        member _.Return(Feed.SkipAndReset) = cont [] FeedType.Reset
        member _.Return(Feed.SkipAndResetFeedback) = cont [] FeedType.ResetFeedback
        member _.Return(Feed.SkipAndResetDescendants feedback) = cont [] (FeedType.ResetDescendants feedback)
        member _.Return(Feed.Stop) = returnFeedRes (Res.Stop [])
    
    let loop = LoopBuilder()
    let feed = FeedBuilder()


    // -------
    // Kleisli composition
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

    
    // --------
    // map / apply / transformation
    // --------

    let map2 (proj: 'v -> LoopState<'s> option -> 'o) (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            let mapValues values state = [ for v in values do proj v state ]
            match run inputGen state with
            | Res.Continue (values, state) ->
                Res.Continue (mapValues values (Some state), state)
            | Res.Stop values ->
                Res.Stop (mapValues values None)
        |> createLoop

    let map proj (inputGen: LoopGen<_,_>) =
        map2 (fun v _ -> proj v) inputGen

    let apply xGen fGen =
        loop {
            let! l' = xGen
            let! f' = fGen
            let result = f' l'
            yield result
        }

    
    // -------
    // count
    // -------
    
    let inline count inclusiveStart increment =
        feed {
            let! curr = Init inclusiveStart
            yield curr, curr + increment
        }

    let inline countToAndThen inclusiveStart increment inclusiveEnd onEnd =
        loop {
            let! c = count inclusiveStart increment
            match c <= inclusiveEnd with
            | true -> yield c
            | false -> return! (createLoop (fun _ -> onEnd))
        }

    let inline countTo inclusiveStart increment inclusiveEnd =
        countToAndThen inclusiveStart increment inclusiveEnd Res.Loop.stop
    
    let inline countToCyclic inclusiveStart increment inclusiveEnd =
        countToAndThen inclusiveStart increment inclusiveEnd Res.Loop.skipAndReset


    // --------
    // onStop trigger
    // --------

    type OnStopThenState<'s> =
        | RunInput of 's option
        | UseDefault

    let inline internal onStopThenValues defaultValues (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        fun state ->
            let state = state |> Option.defaultValue (RunInput None)
            match state with
            | UseDefault ->
                Res.Continue (defaultValues, LoopState.Update UseDefault)
            | RunInput state ->
                let continueWith values state = Res.Loop.emitMany values (RunInput state)
                match run inputGen state with
                | Res.Continue (values, LoopStateToOption None state) ->
                    continueWith values state
                | Res.Stop values ->
                    Res.Loop.emitMany values UseDefault
        |> createLoop
        
    let inline onStopThenDefault defaultValue (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenValues [defaultValue] inputGen

    let inline onStopThenSkip (inputGen: LoopGen<_,_>) : LoopGen<_,_> =
        onStopThenValues [] inputGen


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
